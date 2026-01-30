using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FDAAPI.App.FeatG75_OptimizedRoute
{
    public class OptimizedRouteHandler : IRequestHandler<OptimizedRouteRequest, OptimizedRouteResponse>
    {
        private readonly IGraphHopperService _graphHopper;
        private readonly IRouteFloodAnalyzer _floodAnalyzer;
        private readonly ISafeRouteMapper _mapper;
        private readonly AppDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly ILogger<OptimizedRouteHandler> _logger;

        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public OptimizedRouteHandler(
            IGraphHopperService graphHopper,
            IRouteFloodAnalyzer floodAnalyzer,
            ISafeRouteMapper mapper,
            AppDbContext dbContext,
            IMemoryCache cache,
            ILogger<OptimizedRouteHandler> logger)
        {
            _graphHopper = graphHopper;
            _floodAnalyzer = floodAnalyzer;
            _mapper = mapper;
            _dbContext = dbContext;
            _cache = cache;
            _logger = logger;
        }

        public async Task<OptimizedRouteResponse> Handle(
            OptimizedRouteRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Check cache
                var cacheKey = BuildCacheKey(request);
                if (_cache.TryGetValue(cacheKey, out OptimizedRouteResponse? cached) && cached != null)
                {
                    _logger.LogInformation("Cache HIT for route request: {CacheKey}", cacheKey);
                    cached.Data!.Metadata.Cached = true;
                    return cached;
                }

                _logger.LogInformation("Cache MISS for route request: {CacheKey}", cacheKey);

                // 2. Get all stations with coordinates
                var stations = await _dbContext.Stations
                    .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
                    .ToListAsync(ct);

                var stationIds = stations.Select(s => s.Id).ToList();

                // 3. Get latest sensor reading per station
                var latestReadings = await _dbContext.SensorReadings
                    .Where(sr => stationIds.Contains(sr.StationId))
                    .GroupBy(sr => sr.StationId)
                    .Select(g => new
                    {
                        StationId = g.Key,
                        LatestReading = g.OrderByDescending(sr => sr.MeasuredAt).FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.StationId, x => x.LatestReading!, ct);

                // 4. Build flood polygons (with trend if departureTime provided)
                List<FloodPolygon> floodPolygons;
                bool trendApplied = false;

                if (request.DepartureTime.HasValue)
                {
                    // Get last 3 readings per station for trend analysis
                    var recentReadings = await _dbContext.SensorReadings
                        .Where(sr => stationIds.Contains(sr.StationId))
                        .GroupBy(sr => sr.StationId)
                        .Select(g => new
                        {
                            StationId = g.Key,
                            Readings = g.OrderByDescending(sr => sr.MeasuredAt).Take(3).ToList()
                        })
                        .ToDictionaryAsync(x => x.StationId, x => x.Readings, ct);

                    floodPolygons = _floodAnalyzer.BuildFloodPolygonsWithTrend(
                        stations, latestReadings, recentReadings);
                    trendApplied = true;
                }
                else
                {
                    floodPolygons = _floodAnalyzer.BuildFloodPolygons(stations, latestReadings);
                }

                _logger.LogInformation("Built {Count} flood polygons (trend={Trend})",
                    floodPolygons.Count, trendApplied);

                // 5. Build points array (start + waypoints + end)
                var pointsList = new List<decimal[]>
                {
                    new[] { request.StartLongitude, request.StartLatitude }
                };

                if (request.Waypoints != null)
                {
                    foreach (var wp in request.Waypoints)
                    {
                        pointsList.Add(new[] { wp.Longitude, wp.Latitude });
                    }
                }

                pointsList.Add(new[] { request.EndLongitude, request.EndLatitude });

                var points = pointsList.ToArray();
                var profile = request.RouteProfile.ToLower();
                var hasFloodZones = request.AvoidFloodedAreas && floodPolygons.Any();
                var hasWaypoints = request.Waypoints != null && request.Waypoints.Any();

                // 6. Call GraphHopper
                //    - Safe route: flexible mode + avoid polygons
                //    - Shortest route: flexible mode + distance_influence=200
                //    - Normal route with alternatives: CH mode (ONLY when NO waypoints)
                var safeRouteRequest = new GraphHopperRouteRequest
                {
                    Points = points,
                    Profile = profile,
                    AvoidPolygons = hasFloodZones
                        ? floodPolygons.Select(p => p.Geometry).ToList()
                        : null
                };

                var shortestRouteRequest = new GraphHopperRouteRequest
                {
                    Points = points,
                    Profile = profile,
                    DistanceInfluence = 200
                };

                var safeRouteTask = _graphHopper.GetRouteAsync(safeRouteRequest, ct);
                var shortestRouteTask = _graphHopper.GetRouteAsync(shortestRouteRequest, ct);

                // Normal route with alternatives: only when no waypoints
                Task<GraphHopperRouteResponse>? normalRouteTask = null;
                if (!hasWaypoints)
                {
                    var normalRouteRequest = new GraphHopperRouteRequest
                    {
                        Points = points,
                        Profile = profile,
                        AlternativeRoute = new AlternativeRouteConfig
                        {
                            MaxPaths = request.MaxAlternatives
                        }
                    };
                    normalRouteTask = _graphHopper.GetRouteAsync(normalRouteRequest, ct);
                }

                // Await all tasks
                GraphHopperRouteResponse safeRouteResponse;
                GraphHopperRouteResponse? normalRouteResponse = null;
                GraphHopperRouteResponse? shortestRouteResponse = null;

                try
                {
                    if (normalRouteTask != null)
                    {
                        await Task.WhenAll(safeRouteTask, normalRouteTask, shortestRouteTask);
                        normalRouteResponse = normalRouteTask.Result;
                    }
                    else
                    {
                        await Task.WhenAll(safeRouteTask, shortestRouteTask);
                    }

                    safeRouteResponse = safeRouteTask.Result;
                    shortestRouteResponse = shortestRouteTask.Result;
                }
                catch
                {
                    safeRouteResponse = await safeRouteTask;
                    try { shortestRouteResponse = await shortestRouteTask; } catch { }
                    if (normalRouteTask != null)
                    {
                        try { normalRouteResponse = await normalRouteTask; } catch { }
                    }
                    _logger.LogWarning("Some route requests failed, continuing with available routes");
                }

                // 7. Handle no route found
                if (safeRouteResponse.Paths == null || !safeRouteResponse.Paths.Any())
                {
                    return new OptimizedRouteResponse
                    {
                        Success = false,
                        Message = "No route found. All paths may be blocked by flooding.",
                        StatusCode = SafeRouteStatusCode.RouteBlocked
                    };
                }

                // 8. Analyze route safety
                var safeRoute = safeRouteResponse.Paths.First();
                var safeGeometry = safeRoute.ToGeoJsonGeometry();
                var safeWarnings = _floodAnalyzer.AnalyzeRoute(safeGeometry, floodPolygons);
                var safetyStatus = _floodAnalyzer.CalculateSafetyStatus(safeWarnings);

                // 9. Build GeoJSON features
                var features = new List<object>();

                // Safe route (primary)
                features.Add(_mapper.BuildRouteFeature(safeRoute, safeWarnings, "safeRoute"));

                // Collect alternatives, deduplicate by distance
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

                // Add alternative features with comparison metadata
                var alternativeCount = 0;
                foreach (var (path, source) in alternativePaths)
                {
                    alternativeCount++;
                    var altGeometry = path.ToGeoJsonGeometry();
                    var altWarnings = _floodAnalyzer.AnalyzeRoute(altGeometry, floodPolygons);

                    features.Add(BuildRouteFeatureWithComparison(
                        path, altWarnings, $"alternativeRoute_{alternativeCount}",
                        safeRoute, safeWarnings));
                }

                // Flood zone features (deduplicated)
                var addedStationIds = new HashSet<Guid>(safeWarnings.Select(w => w.StationId));
                foreach (var warning in safeWarnings)
                {
                    features.Add(_mapper.BuildFloodZoneFeature(warning));
                }

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

                // 10. Build response
                var response = new OptimizedRouteResponse
                {
                    Success = true,
                    Message = "Optimized route calculated successfully",
                    StatusCode = SafeRouteStatusCode.Success,
                    Data = new OptimizedRouteGeoJsonData
                    {
                        Type = "FeatureCollection",
                        Features = features,
                        Metadata = new OptimizedRouteMetadata
                        {
                            SafetyStatus = safetyStatus,
                            TotalFloodZones = floodPolygons.Count,
                            AlternativeRouteCount = alternativeCount,
                            GeneratedAt = DateTime.UtcNow,
                            Cached = false,
                            WaypointCount = request.Waypoints?.Count ?? 0,
                            DepartureTime = request.DepartureTime,
                            FloodTrendApplied = trendApplied
                        }
                    }
                };

                // 11. Cache result
                _cache.Set(cacheKey, response, CacheTtl);
                _logger.LogInformation("Cached route response for {Ttl} minutes", CacheTtl.TotalMinutes);

                return response;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "GraphHopper service error");
                return new OptimizedRouteResponse
                {
                    Success = false,
                    Message = "Routing service is currently unavailable",
                    StatusCode = SafeRouteStatusCode.ServiceUnavailable
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in optimized route calculation");
                return new OptimizedRouteResponse
                {
                    Success = false,
                    Message = "An error occurred while calculating the route",
                    StatusCode = SafeRouteStatusCode.UnknownError
                };
            }
        }

        /// <summary>
        /// Build route feature with comparison metadata vs safe route.
        /// </summary>
        private object BuildRouteFeatureWithComparison(
            GraphHopperPath path,
            List<FloodWarningDto> warnings,
            string featureName,
            GraphHopperPath safeRoute,
            List<FloodWarningDto> safeWarnings)
        {
            var baseFeature = _mapper.BuildRouteFeature(path, warnings, featureName);

            // Calculate comparison deltas
            var timeSaved = (int)(path.Time / 1000) - (int)(safeRoute.Time / 1000);
            var distanceSaved = path.Distance - safeRoute.Distance;
            var riskDelta = CalculateRiskScore(warnings) - CalculateRiskScore(safeWarnings);

            // Wrap with comparison data
            return new
            {
                type = "Feature",
                geometry = new
                {
                    type = "LineString",
                    coordinates = path.Points.Coordinates.Select(c => new[] { c[0], c[1] }).ToArray()
                },
                properties = new
                {
                    name = featureName,
                    distanceMeters = path.Distance,
                    durationSeconds = (int)(path.Time / 1000),
                    floodRiskScore = CalculateRiskScore(warnings),
                    comparison = new
                    {
                        timeSavedSeconds = timeSaved,
                        distanceSavedMeters = Math.Round(distanceSaved, 1),
                        floodRiskDelta = riskDelta
                    },
                    instructions = path.Instructions.Select(i => new
                    {
                        distance = i.Distance,
                        time = i.Time,
                        text = i.Text
                    }).ToArray()
                }
            };
        }

        private int CalculateRiskScore(List<FloodWarningDto> warnings)
        {
            if (!warnings.Any()) return 0;
            var critical = warnings.Count(w => w.Severity == "critical");
            var warning = warnings.Count(w => w.Severity == "warning");
            var caution = warnings.Count(w => w.Severity == "caution");
            return Math.Min(100, (critical * 40) + (warning * 20) + (caution * 10));
        }

        /// <summary>
        /// Build deterministic cache key from request parameters.
        /// DepartureTime rounded to 5-minute window.
        /// </summary>
        private string BuildCacheKey(OptimizedRouteRequest request)
        {
            var parts = new List<string>
            {
                $"{request.StartLatitude:F6}",
                $"{request.StartLongitude:F6}",
                $"{request.EndLatitude:F6}",
                $"{request.EndLongitude:F6}",
                request.RouteProfile,
                request.AvoidFloodedAreas.ToString(),
                request.MaxAlternatives.ToString()
            };

            if (request.Waypoints != null)
            {
                foreach (var wp in request.Waypoints)
                    parts.Add($"{wp.Latitude:F6},{wp.Longitude:F6}");
            }

            if (request.DepartureTime.HasValue)
            {
                // Round to 5-minute window
                var ticks = request.DepartureTime.Value.Ticks;
                var rounded = new DateTime(
                    ticks / (TimeSpan.TicksPerMinute * 5) * (TimeSpan.TicksPerMinute * 5),
                    DateTimeKind.Utc);
                parts.Add(rounded.ToString("O"));
            }

            var raw = string.Join("|", parts);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return $"optimized-route:{Convert.ToHexString(hash)}";
        }
    }
}
