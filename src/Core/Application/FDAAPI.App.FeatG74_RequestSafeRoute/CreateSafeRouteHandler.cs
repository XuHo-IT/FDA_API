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

                // 4. Call GraphHopper with avoid_polygons
                var routeRequest = new GraphHopperRouteRequest
                {
                    Points = new[]
                    {
                        new[] { request.StartLongitude, request.StartLatitude },
                        new[] { request.EndLongitude, request.EndLatitude }
                    },
                    Profile = request.RouteProfile.ToLower(),
                    AvoidPolygons = request.AvoidFloodedAreas && floodPolygons.Any()
                        ? floodPolygons.Select(p => p.Geometry).ToList()
                        : null,
                    AlternativeRoute = request.MaxAlternatives > 0
                        ? new AlternativeRouteConfig { MaxPaths = request.MaxAlternatives }
                        : null
                };

                var routeResponse = await _graphHopper.GetRouteAsync(routeRequest, ct);

                // 5. Handle no route found
                if (routeResponse.Paths == null || !routeResponse.Paths.Any())
                {
                    return new SafeRouteResponse
                    {
                        Success = false,
                        Message = "No route found. All paths may be blocked by flooding.",
                        StatusCode = SafeRouteStatusCode.RouteBlocked
                    };
                }

                // 6. Analyze route safety
                var primaryRoute = routeResponse.Paths.First();
                var primaryGeometry = primaryRoute.ToGeoJsonGeometry();
                var floodWarnings = _floodAnalyzer.AnalyzeRoute(primaryGeometry, floodPolygons);
                var safetyStatus = _floodAnalyzer.CalculateSafetyStatus(floodWarnings);

                // 7. Build GeoJSON FeatureCollection response
                var features = new List<object>();

                // Primary route feature
                features.Add(_mapper.BuildRouteFeature(
                    primaryRoute, floodWarnings, "primaryRoute"));

                // Alternative route features
                var altIndex = 1;
                foreach (var altPath in routeResponse.Paths.Skip(1))
                {
                    var altWarnings = _floodAnalyzer.AnalyzeRoute(
                        altPath.ToGeoJsonGeometry(), floodPolygons);
                    features.Add(_mapper.BuildRouteFeature(
                        altPath, altWarnings, $"alternativeRoute_{altIndex++}"));
                }

                // Flood zone features
                foreach (var warning in floodWarnings)
                {
                    features.Add(_mapper.BuildFloodZoneFeature(warning));
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
                            TotalFloodZones = floodWarnings.Count,
                            AlternativeRouteCount = routeResponse.Paths.Count - 1,
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
