using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Map;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.App.FeatG31_GetMapCurrentStatus
{
    public class GetMapCurrentStatusHandler : IRequestHandler<GetMapCurrentStatusRequest, GetMapCurrentStatusResponse>
    {
        private readonly IStationRepository _stationRepository;
        private readonly AppDbContext _dbContext;
        private readonly IGraphHopperService _graphHopperService;

        public GetMapCurrentStatusHandler(
            IStationRepository stationRepository,
            AppDbContext dbContext,
            IGraphHopperService graphHopperService)
        {
            _stationRepository = stationRepository;
            _dbContext = dbContext;
            _graphHopperService = graphHopperService;
        }

        public async Task<GetMapCurrentStatusResponse> Handle(
            GetMapCurrentStatusRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Get all stations with optional bounds filter
                var stationsQuery = _dbContext.Stations
                    .Where(s => s.Latitude.HasValue && s.Longitude.HasValue);

                // Filter by status if provided
                if (!string.IsNullOrEmpty(request.Status))
                {
                    stationsQuery = stationsQuery.Where(s => s.Status == request.Status);
                }

                // Filter by geographic bounds if provided
                if (request.MinLat.HasValue && request.MaxLat.HasValue &&
                    request.MinLng.HasValue && request.MaxLng.HasValue)
                {
                    stationsQuery = stationsQuery.Where(s =>
                        s.Latitude >= request.MinLat &&
                        s.Latitude <= request.MaxLat &&
                        s.Longitude >= request.MinLng &&
                        s.Longitude <= request.MaxLng);
                }

                var stations = await stationsQuery.ToListAsync(ct);

                // 2. Get latest sensor reading for each station (optimized query)
                var stationIds = stations.Select(s => s.Id).ToList();

                // Get the latest reading for each station using GROUP BY and subquery
                var latestReadings = await _dbContext.SensorReadings
                    .Where(sr => stationIds.Contains(sr.StationId))
                    .GroupBy(sr => sr.StationId)
                    .Select(g => new
                    {
                        StationId = g.Key,
                        LatestReading = g.OrderByDescending(sr => sr.MeasuredAt).FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.StationId, x => x.LatestReading, ct);

                // 3. Build station flood status with real sensor data
                var stationStatuses = stations.Select(station =>
                {
                    var latestReading = latestReadings.GetValueOrDefault(station.Id);
                    var (severity, level) = latestReading != null
                        ? CalculateFloodSeverity(latestReading.Value)
                        : ("unknown", -1);

                    return new StationFloodStatus
                    {
                        StationId = station.Id,
                        StationCode = station.Code,
                        StationName = station.Name,
                        LocationDesc = station.LocationDesc ?? "",
                        RoadName = station.RoadName ?? "",
                        WaterLevel = latestReading?.Value,
                        Distance = latestReading?.Distance,
                        SensorHeight = latestReading?.SensorHeight,
                        Unit = latestReading?.Unit ?? "cm",
                        MeasuredAt = latestReading?.MeasuredAt,
                        Severity = severity,
                        SeverityLevel = level,
                        StationStatus = station.Status ?? "unknown",
                        LastSeenAt = station.LastSeenAt,
                        Latitude = station.Latitude,
                        Longitude = station.Longitude
                    };
                }).ToList();

                // 4. Convert to GeoJSON FeatureCollection
                var features = stationStatuses
                    .Where(s => stations.Any(st => st.Id == s.StationId))
                    .Select(status =>
                    {
                        var station = stations.First(s => s.Id == status.StationId);

                        return new GeoJsonFeature
                        {
                            Type = "Feature",
                            Geometry = new GeoJsonGeometry
                            {
                                Type = "Point",
                                Coordinates = new[] { station.Longitude!.Value, station.Latitude!.Value }
                            },
                            Properties = new
                            {
                                // Station info
                                stationId = status.StationId,
                                stationCode = status.StationCode,
                                stationName = status.StationName,
                                locationDesc = status.LocationDesc,
                                roadName = status.RoadName,

                                // Water level data
                                waterLevel = status.WaterLevel,
                                distance = status.Distance,
                                sensorHeight = status.SensorHeight,
                                unit = status.Unit,
                                measuredAt = status.MeasuredAt,

                                // Flood severity
                                severity = status.Severity,
                                severityLevel = status.SeverityLevel,

                                // Station status
                                stationStatus = status.StationStatus,
                                lastSeenAt = status.LastSeenAt,

                                // Additional info for map styling
                                markerColor = GetMarkerColor(status.SeverityLevel),
                                alertLevel = GetAlertLevel(status.SeverityLevel)
                            }
                        };
                    })
                    .ToList();

                // 5. Build road segment LineString features using GraphHopper road geometry
                var roadSegmentFeatures = new List<GeoJsonFeature>();
                var stationsWithCoords = stationStatuses
                    .Where(s => s.Latitude.HasValue && s.Longitude.HasValue && !string.IsNullOrEmpty(s.RoadName))
                    .GroupBy(s => s.RoadName);

                // Collect all station pairs
                var stationPairs = new List<(StationFloodStatus stA, StationFloodStatus stB)>();
                foreach (var road in stationsWithCoords)
                {
                    var stationList = road.ToList();
                    if (stationList.Count < 2) continue;
                    var ordered = OrderStationsByProximity(stationList);
                    for (int i = 0; i < ordered.Count - 1; i++)
                        stationPairs.Add((ordered[i], ordered[i + 1]));
                }

                // Call GraphHopper in parallel for all pairs
                var routeTasks = stationPairs.Select(async pair =>
                {
                    var (stA, stB) = pair;
                    decimal[][] routeCoords;
                    try
                    {
                        var routeResponse = await _graphHopperService.GetRouteAsync(new GraphHopperRouteRequest
                        {
                            Points = new[]
                            {
                                new[] { stA.Longitude!.Value, stA.Latitude!.Value },
                                new[] { stB.Longitude!.Value, stB.Latitude!.Value }
                            },
                            Profile = "car",
                            Instructions = false
                        }, ct);

                        var path = routeResponse.Paths.FirstOrDefault();
                        routeCoords = path?.Points.Coordinates.Length >= 2
                            ? path.Points.Coordinates
                                .Select(c => new[] { (decimal)c[0], (decimal)c[1] })
                                .ToArray()
                            : FallbackCoords(stA, stB);
                    }
                    catch
                    {
                        routeCoords = FallbackCoords(stA, stB);
                    }

                    return new GeoJsonFeature
                    {
                        Type = "Feature",
                        Geometry = new GeoJsonLineStringGeometry { Coordinates = routeCoords },
                        Properties = new
                        {
                            roadName = stA.RoadName,
                            startStationId = stA.StationId,
                            endStationId = stB.StationId,
                            startSeverityLevel = stA.SeverityLevel,
                            endSeverityLevel = stB.SeverityLevel,
                            startColor = GetMarkerColor(stA.SeverityLevel),
                            endColor = GetMarkerColor(stB.SeverityLevel)
                        }
                    };
                });

                roadSegmentFeatures.AddRange(await Task.WhenAll(routeTasks));

                var allFeatures = features.Concat(roadSegmentFeatures).ToList();

                return new GetMapCurrentStatusResponse
                {
                    Success = true,
                    Message = $"Retrieved {features.Count} stations with current status",
                    StatusCode = GetMapCurrentStatusResponseStatusCode.Success,
                    Data = new GeoJsonFeatureCollection
                    {
                        Type = "FeatureCollection",
                        Features = allFeatures,
                        Metadata = new
                        {
                            totalStations = features.Count,
                            roadSegments = roadSegmentFeatures.Count,
                            stationsWithData = features.Count(f => ((dynamic)f.Properties!).waterLevel != null),
                            stationsNoData = features.Count(f => ((dynamic)f.Properties!).waterLevel == null),
                            generatedAt = DateTime.UtcNow,
                            bounds = request.MinLat.HasValue ? new
                            {
                                minLat = request.MinLat,
                                maxLat = request.MaxLat,
                                minLng = request.MinLng,
                                maxLng = request.MaxLng
                            } : null
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new GetMapCurrentStatusResponse
                {
                    Success = false,
                    Message = $"Error retrieving map current status: {ex.Message}",
                    StatusCode = GetMapCurrentStatusResponseStatusCode.UnknownError
                };
            }
        }

        /// <summary>
        /// Calculate flood severity based on water level
        /// Thresholds can be made configurable later
        /// </summary>
        private (string severity, int level) CalculateFloodSeverity(double waterLevel)
        {
            // Convert to meters if in cm
            double waterLevelInMeters = waterLevel / 100.0; // Assuming input is in cm

            // Severity thresholds (configurable in production)
            if (waterLevelInMeters >= 0.4)
                return ("critical", 3);  // Severe flooding

            if (waterLevelInMeters >= 0.2)
                return ("warning", 2);   // High water level

            if (waterLevelInMeters >= 0.1)
                return ("caution", 1);   // Elevated water level

            return ("safe", 0);          // Normal water level
        }

        /// <summary>
        /// Get marker color for map visualization
        /// </summary>
        private string GetMarkerColor(int severityLevel)
        {
            return severityLevel switch
            {
                3 => "#DC2626",  // Red - Critical
                2 => "#F97316",  // Orange - Warning
                1 => "#FCD34D",  // Yellow - Caution
                0 => "#10B981",  // Green - Safe
                _ => "#9CA3AF"   // Gray - No data
            };
        }

        /// <summary>
        /// Get alert level text for UI display
        /// </summary>
        private string GetAlertLevel(int severityLevel)
        {
            return severityLevel switch
            {
                3 => "CRITICAL",
                2 => "WARNING",
                1 => "CAUTION",
                0 => "SAFE",
                _ => "NO DATA"
            };
        }

        private static decimal[][] FallbackCoords(StationFloodStatus stA, StationFloodStatus stB)
        {
            return new[]
            {
                new[] { stA.Longitude!.Value, stA.Latitude!.Value },
                new[] { stB.Longitude!.Value, stB.Latitude!.Value }
            };
        }

        /// <summary>
        /// Order stations along a road using nearest-neighbor chain starting from the westernmost station.
        /// </summary>
        private List<StationFloodStatus> OrderStationsByProximity(List<StationFloodStatus> stations)
        {
            var remaining = stations.ToList();
            var ordered = new List<StationFloodStatus>();
            var current = remaining.MinBy(s => s.Longitude);
            remaining.Remove(current!);
            ordered.Add(current!);

            while (remaining.Count > 0)
            {
                var next = remaining.MinBy(s =>
                    Math.Pow((double)(s.Latitude! - current!.Latitude!), 2) +
                    Math.Pow((double)(s.Longitude! - current!.Longitude!), 2));
                remaining.Remove(next!);
                ordered.Add(next!);
                current = next;
            }
            return ordered;
        }
    }
}