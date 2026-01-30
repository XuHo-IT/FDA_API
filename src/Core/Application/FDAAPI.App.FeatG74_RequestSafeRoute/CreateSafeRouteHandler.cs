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

                // 4. Call GraphHopper 3 times in parallel:
                //    - Safe route: avoids flood zones (flexible mode)
                //    - Normal fastest route: with alternatives (CH mode)
                //    - Shortest route: shortest distance (CH mode, weighting=shortest)
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
                    Profile = profile,
                    AlternativeRoute = new AlternativeRouteConfig
                    {
                        MaxPaths = request.MaxAlternatives
                    }
                };

                var shortestRouteRequest = new GraphHopperRouteRequest
                {
                    Points = points,
                    Profile = profile,
                    DistanceInfluence = 200
                };

                var safeRouteTask = _graphHopper.GetRouteAsync(safeRouteRequest, ct);
                var normalRouteTask = _graphHopper.GetRouteAsync(normalRouteRequest, ct);
                var shortestRouteTask = _graphHopper.GetRouteAsync(shortestRouteRequest, ct);

                GraphHopperRouteResponse safeRouteResponse;
                GraphHopperRouteResponse? normalRouteResponse = null;
                GraphHopperRouteResponse? shortestRouteResponse = null;

                try
                {
                    await Task.WhenAll(safeRouteTask, normalRouteTask, shortestRouteTask);
                    safeRouteResponse = safeRouteTask.Result;
                    normalRouteResponse = normalRouteTask.Result;
                    shortestRouteResponse = shortestRouteTask.Result;
                }
                catch
                {
                    safeRouteResponse = await safeRouteTask;
                    try { normalRouteResponse = await normalRouteTask; } catch { }
                    try { shortestRouteResponse = await shortestRouteTask; } catch { }
                    _logger.LogWarning("Some route requests failed, continuing with available routes");
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

                // Collect all alternative paths (normal + shortest), deduplicate by distance
                var alternativePaths = new List<(GraphHopperPath Path, string Source)>();
                var addedDistances = new HashSet<double> { Math.Round(safeRoute.Distance, 1) };

                if (normalRouteResponse?.Paths != null)
                {
                    foreach (var path in normalRouteResponse.Paths)
                    {
                        var roundedDist = Math.Round(path.Distance, 1);
                        if (addedDistances.Add(roundedDist))
                            alternativePaths.Add((path, "normalRoute"));
                    }
                }

                if (shortestRouteResponse?.Paths != null)
                {
                    foreach (var path in shortestRouteResponse.Paths)
                    {
                        var roundedDist = Math.Round(path.Distance, 1);
                        if (addedDistances.Add(roundedDist))
                            alternativePaths.Add((path, "shortestRoute"));
                    }
                }

                // Add alternative route features with sequential numbering
                var alternativeCount = 0;
                foreach (var (path, source) in alternativePaths)
                {
                    alternativeCount++;
                    var altGeometry = path.ToGeoJsonGeometry();
                    var altWarnings = _floodAnalyzer.AnalyzeRoute(altGeometry, floodPolygons);
                    features.Add(_mapper.BuildRouteFeature(
                        path, altWarnings, $"alternativeRoute_{alternativeCount}"));
                }

                // Flood zone features from safe route
                foreach (var warning in safeWarnings)
                {
                    features.Add(_mapper.BuildFloodZoneFeature(warning));
                }

                // Also include flood zones from alternative routes (deduplicated)
                var addedStationIds = new HashSet<Guid>(safeWarnings.Select(w => w.StationId));
                foreach (var (path, _) in alternativePaths)
                {
                    var altGeometry = path.ToGeoJsonGeometry();
                    var altWarnings = _floodAnalyzer.AnalyzeRoute(altGeometry, floodPolygons);
                    foreach (var warning in altWarnings)
                    {
                        if (addedStationIds.Add(warning.StationId))
                        {
                            features.Add(_mapper.BuildFloodZoneFeature(warning));
                        }
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
                            AlternativeRouteCount = alternativeCount,
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
