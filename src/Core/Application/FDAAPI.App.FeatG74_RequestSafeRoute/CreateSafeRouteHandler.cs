using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.App.Common.Services;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG74_RequestSafeRoute
{
    public class CreateSafeRouteHandler : IRequestHandler<CreateSafeRouteRequest, SafeRouteResponse>
    {
        private readonly IGraphHopperService _graphHopper;
        private readonly IStationRepository _stationRepository;
        private readonly IRouteFloodAnalyzer _floodAnalyzer;
        private readonly ISafeRouteMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<CreateSafeRouteHandler> _logger;

        public CreateSafeRouteHandler(
            IGraphHopperService graphHopper,
            IStationRepository stationRepository,
            IRouteFloodAnalyzer floodAnalyzer,
            ISafeRouteMapper mapper,
            AppDbContext dbContext,
            ILogger<CreateSafeRouteHandler> logger)
        {
            _graphHopper = graphHopper;
            _stationRepository = stationRepository;
            _floodAnalyzer = floodAnalyzer;
            _mapper = mapper;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<SafeRouteResponse> Handle(
            CreateSafeRouteRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Get all stations with coordinates (matching FeatG31 pattern)
                var stations = await _dbContext.Stations
                    .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
                    .ToListAsync(ct);

                _logger.LogInformation("Found {Count} stations with coordinates", stations.Count);

                // 2. Get latest sensor reading for each station (matching FeatG31 pattern)
                var stationIds = stations.Select(s => s.Id).ToList();

                var latestReadings = await _dbContext.SensorReadings
                    .Where(sr => stationIds.Contains(sr.StationId))
                    .GroupBy(sr => sr.StationId)
                    .Select(g => new
                    {
                        StationId = g.Key,
                        LatestReading = g.OrderByDescending(sr => sr.MeasuredAt).FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.StationId, x => x.LatestReading!, ct);

                // 3. Build flood polygons from real-time data
                var floodPolygons = _floodAnalyzer.BuildFloodPolygons(stations, latestReadings);
                _logger.LogInformation("Built {Count} flood avoidance polygons", floodPolygons.Count);

                var points = new[]
                {
                    new[] { request.StartLongitude, request.StartLatitude },
                    new[] { request.EndLongitude, request.EndLatitude }
                };
                var profile = request.RouteProfile.ToLower();
                var hasFloodZones = request.AvoidFloodedAreas && floodPolygons.Any();

                // 4. Call GraphHopper twice in parallel:
                //    - Safe route: avoids flood zones (flexible mode, no alternatives)
                //    - Normal route: fastest/shortest without avoidance (CH mode)
                var safeRouteRequest = new GraphHopperRouteRequest
                {
                    Points = points,
                    Profile = profile,
                    AvoidPolygons = hasFloodZones
                        ? floodPolygons.Select(p => p.Geometry).ToList()
                        : null
                };

                var normalRouteRequest = new GraphHopperRouteRequest
                {
                    Points = points,
                    Profile = profile
                };

                var safeRouteTask = _graphHopper.GetRouteAsync(safeRouteRequest, ct);
                var normalRouteTask = _graphHopper.GetRouteAsync(normalRouteRequest, ct);

                GraphHopperRouteResponse safeRouteResponse;
                GraphHopperRouteResponse? normalRouteResponse = null;

                try
                {
                    await Task.WhenAll(safeRouteTask, normalRouteTask);
                    safeRouteResponse = safeRouteTask.Result;
                    normalRouteResponse = normalRouteTask.Result;
                }
                catch
                {
                    // If parallel fails, at least get safe route
                    safeRouteResponse = await safeRouteTask;
                    _logger.LogWarning("Normal route request failed, continuing with safe route only");
                }

                // 5. Handle no route found
                if (safeRouteResponse.Paths == null || !safeRouteResponse.Paths.Any())
                {
                    return new SafeRouteResponse
                    {
                        Success = false,
                        Message = "No route found. All paths may be blocked by flooding.",
                        StatusCode = SafeRouteStatusCode.RouteBlocked
                    };
                }

                // 6. Analyze route safety
                var safeRoute = safeRouteResponse.Paths.First();
                var safeGeometry = safeRoute.ToGeoJsonGeometry();
                var safeWarnings = _floodAnalyzer.AnalyzeRoute(safeGeometry, floodPolygons);
                var safetyStatus = _floodAnalyzer.CalculateSafetyStatus(safeWarnings);

                // 7. Build GeoJSON FeatureCollection response
                var features = new List<object>();

                // Safe route feature (primary)
                features.Add(_mapper.BuildRouteFeature(
                    safeRoute, safeWarnings, "safeRoute"));

                // Normal route feature (alternative for comparison)
                if (normalRouteResponse?.Paths != null && normalRouteResponse.Paths.Any())
                {
                    var normalRoute = normalRouteResponse.Paths.First();
                    var normalGeometry = normalRoute.ToGeoJsonGeometry();
                    var normalWarnings = _floodAnalyzer.AnalyzeRoute(normalGeometry, floodPolygons);
                    features.Add(_mapper.BuildRouteFeature(
                        normalRoute, normalWarnings, "normalRoute"));
                }

                // Flood zone features
                foreach (var warning in safeWarnings)
                {
                    features.Add(_mapper.BuildFloodZoneFeature(warning));
                }

                // Also include flood zones that safe route avoids but normal route hits
                if (normalRouteResponse?.Paths != null && normalRouteResponse.Paths.Any())
                {
                    var normalGeometry = normalRouteResponse.Paths.First().ToGeoJsonGeometry();
                    var normalWarnings = _floodAnalyzer.AnalyzeRoute(normalGeometry, floodPolygons);
                    var additionalWarnings = normalWarnings
                        .Where(nw => !safeWarnings.Any(sw => sw.StationId == nw.StationId));
                    foreach (var warning in additionalWarnings)
                    {
                        features.Add(_mapper.BuildFloodZoneFeature(warning));
                    }
                }

                return new SafeRouteResponse
                {
                    Success = true,
                    Message = "Route calculated successfully",
                    StatusCode = SafeRouteStatusCode.Success,
                    Data = new SafeRouteGeoJsonData
                    {
                        Type = "FeatureCollection",
                        Features = features,
                        Metadata = new SafeRouteMetadata
                        {
                            SafetyStatus = safetyStatus,
                            TotalFloodZones = floodPolygons.Count,
                            AlternativeRouteCount = normalRouteResponse?.Paths?.Any() == true ? 1 : 0,
                            GeneratedAt = DateTime.UtcNow
                        }
                    }
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "GraphHopper service error");
                return new SafeRouteResponse
                {
                    Success = false,
                    Message = "Routing service is currently unavailable",
                    StatusCode = SafeRouteStatusCode.ServiceUnavailable
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in safe route calculation");
                return new SafeRouteResponse
                {
                    Success = false,
                    Message = "An error occurred while calculating the route",
                    StatusCode = SafeRouteStatusCode.UnknownError
                };
            }
        }
    }

}
