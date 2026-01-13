using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FDAAPI.App.Common.Models.Areas;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG34_AreaStatusEvaluate
{
    public class AreaStatusEvaluateHandler : IRequestHandler<AreaStatusEvaluateRequest, AreaStatusEvaluateResponse>
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IStationRepository _stationRepository;
        private readonly ISensorReadingRepository _sensorReadingRepository;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;

        private const double EarthRadiusKm = 6371.0;

        public AreaStatusEvaluateHandler(
            IAreaRepository areaRepository,
            IStationRepository stationRepository,
            ISensorReadingRepository sensorReadingRepository,
            IDistributedCache cache,
            IConfiguration configuration)
        {
            _areaRepository = areaRepository;
            _stationRepository = stationRepository;
            _sensorReadingRepository = sensorReadingRepository;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<AreaStatusEvaluateResponse> Handle(AreaStatusEvaluateRequest request, CancellationToken ct)
        {
            try
            {
                // 1. Try get from cache
                var cachePrefix = _configuration["Caching:AreaStatusPrefix"] ?? "area_status_";
                var cacheKey = $"{cachePrefix}{request.AreaId}";
                var cachedData = await _cache.GetStringAsync(cacheKey, ct);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var cachedResponse = JsonSerializer.Deserialize<AreaStatusEvaluateResponse>(cachedData);
                    if (cachedResponse != null)
                    {
                        cachedResponse.Message = "Area status retrieved from cache";
                        cachedResponse.StatusCode = AreaStatusCode.Success;
                        return cachedResponse;
                    }
                }

                // 2. Get Area
                var area = await _areaRepository.GetByIdAsync(request.AreaId, ct);
                if (area == null)
                {
                    return new AreaStatusEvaluateResponse
                    {
                        Success = false,
                        Message = "Area not found",
                        StatusCode = AreaStatusCode.NotFound
                    };
                }

                // 3. Get all active stations
                var (stations, totalCount) = await _stationRepository.GetStationsAsync(
                    searchTerm: null,
                    status: "active",
                    pageNumber: 1,
                    pageSize: 1000,
                    ct);

                // 4. Find stations within radius
                var nearbyStations = stations
                    .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
                    .Select(s => new
                    {
                        Station = s,
                        Distance = CalculateDistance(
                            (double)area.Latitude, (double)area.Longitude,
                            (double)s.Latitude!.Value, (double)s.Longitude!.Value) * 1000 // Convert to meters
                    })
                    .Where(s => s.Distance <= area.RadiusMeters)
                    .ToList();

                AreaStatusEvaluateResponse response;

                if (!nearbyStations.Any())
                {
                    response = new AreaStatusEvaluateResponse
                    {
                        Success = true,
                        Message = "No sensors found within area radius",
                        StatusCode = AreaStatusCode.Success,
                        Data = new AreaStatusDto
                        {
                            AreaId = area.Id,
                            Status = "Unknown",
                            SeverityLevel = -1,
                            Summary = "No sensors found within monitoring range.",
                            EvaluatedAt = DateTime.UtcNow
                        }
                    };
                }
                else
                {
                    // 5. Get latest readings for nearby stations
                    var stationIds = nearbyStations.Select(s => s.Station.Id).ToList();
                    var latestReadings = await _sensorReadingRepository.GetLatestReadingsByStationsAsync(stationIds, ct);

                    // 6. Aggregate status
                    var contributingStations = nearbyStations.Select(ns =>
                    {
                        var reading = latestReadings.FirstOrDefault(r => r.StationId == ns.Station.Id);
                        var (severity, weight) = CalculateSeverity(reading?.Value ?? 0, ns.Station);

                        return new ContributingStationDto
                        {
                            StationCode = ns.Station.Code,
                            Distance = Math.Round(ns.Distance, 2),
                            WaterLevel = reading?.Value ?? 0,
                            Severity = severity,
                            Weight = weight
                        };
                    }).ToList();

                    var maxWeight = contributingStations.Max(s => s.Weight);
                    var status = MapWeightToStatus(maxWeight);

                    response = new AreaStatusEvaluateResponse
                    {
                        Success = true,
                        Message = "Area status evaluated successfully",
                        StatusCode = AreaStatusCode.Success,
                        Data = new AreaStatusDto
                        {
                            AreaId = area.Id,
                            Status = status,
                            SeverityLevel = maxWeight,
                            Summary = BuildSummary(status, contributingStations),
                            ContributingStations = contributingStations,
                            EvaluatedAt = DateTime.UtcNow
                        }
                    };
                }

                // 7. Save to cache (30 seconds as recommended)
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions, ct);

                return response;
            }
            catch (Exception ex)
            {
                return new AreaStatusEvaluateResponse
                {
                    Success = false,
                    Message = $"Error evaluating area status: {ex.Message}",
                    StatusCode = AreaStatusCode.InternalServerError
                };
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private double ToRadians(double angle) => Math.PI * angle / 180.0;

        private (string severity, int weight) CalculateSeverity(double waterLevel, Station station)
        {
            // Use station thresholds if available, otherwise defaults
            var warningThreshold = (double)(station.ThresholdWarning ?? 1.0m);
            var criticalThreshold = (double)(station.ThresholdCritical ?? 2.0m);

            if (waterLevel >= criticalThreshold) return ("critical", 3);
            if (waterLevel >= warningThreshold) return ("warning", 2);
            if (waterLevel > 0) return ("safe", 0);
            return ("unknown", -1);
        }

        private string MapWeightToStatus(int weight)
        {
            return weight switch
            {
                3 => "Warning", // Critical level 
                2 => "Watch",   // Warning level 
                0 => "Normal",  // Safe level 
                _ => "Unknown"  // No readings or unknown
            };
        }

        private string BuildSummary(string status, List<ContributingStationDto> stations)
        {
            if (status == "Warning")
            {
                var critical = stations.OrderByDescending(s => s.Weight).ThenBy(s => s.Distance).First();
                return $"Warning: Critical water level detected at Station {critical.StationCode} ({critical.Distance}m away).";
            }
            if (status == "Watch")
            {
                var warning = stations.OrderByDescending(s => s.Weight).ThenBy(s => s.Distance).First();
                return $"Watch: High water level detected at Station {warning.StationCode} ({warning.Distance}m away).";
            }
            if (status == "Normal")
            {
                return "Area status is currently Normal. All nearby sensors report safe levels.";
            }
            return "Area status is Unknown due to lack of recent sensor data.";
        }
    }
}

