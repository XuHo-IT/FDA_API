using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Areas;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG55_AdministrativeAreasEvaluate
{
    public class AdministrativeAreasEvaluateHandler : IRequestHandler<AdministrativeAreasEvaluateRequest, AdministrativeAreasEvaluateResponse>
    {
        private readonly IAdministrativeAreaRepository _administrativeAreaRepository;
        private readonly IStationRepository _stationRepository;
        private readonly ISensorReadingRepository _sensorReadingRepository;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;

        public AdministrativeAreasEvaluateHandler(
            IAdministrativeAreaRepository administrativeAreaRepository,
            IStationRepository stationRepository,
            ISensorReadingRepository sensorReadingRepository,
            IDistributedCache cache,
            IConfiguration configuration)
        {
            _administrativeAreaRepository = administrativeAreaRepository;
            _stationRepository = stationRepository;
            _sensorReadingRepository = sensorReadingRepository;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<AdministrativeAreasEvaluateResponse> Handle(AdministrativeAreasEvaluateRequest request, CancellationToken ct)
        {
            try
            {
                // 1. Try get from cache (include level in cache key for proper separation)
                var administrativeArea = await _administrativeAreaRepository.GetByIdAsync(request.AdministrativeAreaId, ct);
                if (administrativeArea == null)
                {
                    return new AdministrativeAreasEvaluateResponse
                    {
                        Success = false,
                        Message = "Administrative area not found",
                        StatusCode = AreaStatusCode.NotFound
                    };
                }

                var cachePrefix = _configuration["Caching:AdministrativeAreaStatusPrefix"] ?? "admin_area_status_";
                var cacheKey = $"{cachePrefix}{administrativeArea.Level.ToLower()}_{request.AdministrativeAreaId}";
                var cachedData = await _cache.GetStringAsync(cacheKey, ct);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var cachedResponse = JsonSerializer.Deserialize<AdministrativeAreasEvaluateResponse>(cachedData);
                    if (cachedResponse != null)
                    {
                        cachedResponse.Message = "Administrative area status retrieved from cache";
                        cachedResponse.StatusCode = AreaStatusCode.Success;
                        return cachedResponse;
                    }
                }

                // 2. Process based on level
                AdministrativeAreasEvaluateResponse response;
                
                switch (administrativeArea.Level.ToLower())
                {
                    case "ward":
                        response = await EvaluateWardAsync(administrativeArea, ct);
                        break;
                    case "district":
                        response = await EvaluateDistrictAsync(administrativeArea, ct);
                        break;
                    case "city":
                        response = await EvaluateCityAsync(administrativeArea, ct);
                        break;
                    default:
                        return new AdministrativeAreasEvaluateResponse
                        {
                            Success = false,
                            Message = $"Unsupported administrative area level: {administrativeArea.Level}",
                            StatusCode = AreaStatusCode.BadRequest
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
                return new AdministrativeAreasEvaluateResponse
                {
                    Success = false,
                    Message = $"Error evaluating administrative area status: {ex.Message}",
                    StatusCode = AreaStatusCode.InternalServerError
                };
            }
        }

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
                return $"Warning: Critical water level detected at Station {critical.StationCode}.";
            }
            if (status == "Watch")
            {
                var warning = stations.OrderByDescending(s => s.Weight).ThenBy(s => s.Distance).First();
                return $"Watch: High water level detected at Station {warning.StationCode}.";
            }
            if (status == "Normal")
            {
                return "Administrative area status is currently Normal. All sensors in the area report safe levels.";
            }
            return "Administrative area status is Unknown due to lack of recent sensor data.";
        }

        private AdministrativeAreaInfoDto BuildAdministrativeAreaInfo(AdministrativeArea area, AdministrativeArea? parentArea)
        {
            return new AdministrativeAreaInfoDto
            {
                Id = area.Id,
                Name = area.Name,
                Level = area.Level,
                Code = area.Code,
                ParentId = area.ParentId,
                ParentName = parentArea?.Name
            };
        }

        private object? ParseGeoJson(string? geometryJson)
        {
            if (string.IsNullOrWhiteSpace(geometryJson))
                return null;

            try
            {
                // Parse the JSON string to object
                return JsonSerializer.Deserialize<object>(geometryJson);
            }
            catch
            {
                // If parsing fails, return null
                return null;
            }
        }

        // Ward: Get stations directly in the ward
        private async Task<AdministrativeAreasEvaluateResponse> EvaluateWardAsync(AdministrativeArea ward, CancellationToken ct)
        {
            // Get parent area once
            AdministrativeArea? wardParentArea = ward.ParentId.HasValue 
                ? await _administrativeAreaRepository.GetByIdAsync(ward.ParentId.Value, ct) 
                : null;

            // Get all active stations
            var (allStations, totalCount) = await _stationRepository.GetStationsAsync(
                searchTerm: null,
                status: "active",
                pageNumber: 1,
                pageSize: 1000,
                ct);

            var stationsInWard = allStations
                .Where(s => s.AdministrativeAreaId == ward.Id)
                .ToList();

            if (!stationsInWard.Any())
            {

                return new AdministrativeAreasEvaluateResponse
                {
                    Success = true,
                    Message = "No sensors found in this ward",
                    StatusCode = AreaStatusCode.Success,
                    Data = new AdministrativeAreaStatusDto
                    {
                        AdministrativeAreaId = ward.Id,
                        Status = "Unknown",
                        SeverityLevel = -1,
                        Summary = "No sensors found in this ward.",
                        EvaluatedAt = DateTime.UtcNow,
                        AdministrativeArea = BuildAdministrativeAreaInfo(ward, wardParentArea),
                        GeoJson = ParseGeoJson(ward.Geometry)
                    }
                };
            }

            // Get latest readings
            var stationIds = stationsInWard.Select(s => s.Id).ToList();
            var latestReadings = await _sensorReadingRepository.GetLatestReadingsByStationsAsync(stationIds, ct);

            // Aggregate status
            var contributingStations = stationsInWard.Select(station =>
            {
                var reading = latestReadings.FirstOrDefault(r => r.StationId == station.Id);
                var (severity, weight) = CalculateSeverity(reading?.Value ?? 0, station);

                return new ContributingStationDto
                {
                    StationId = station.Id,
                    StationCode = station.Code,
                    Distance = 0,
                    WaterLevel = reading?.Value ?? 0,
                    Severity = severity,
                    Weight = weight
                };
            }).ToList();

            var maxWeight = contributingStations.Any() ? contributingStations.Max(s => s.Weight) : -1;
            var status = MapWeightToStatus(maxWeight);

            return new AdministrativeAreasEvaluateResponse
            {
                Success = true,
                Message = "Ward status evaluated successfully",
                StatusCode = AreaStatusCode.Success,
                Data = new AdministrativeAreaStatusDto
                {
                    AdministrativeAreaId = ward.Id,
                    Status = status,
                    SeverityLevel = maxWeight,
                    Summary = BuildSummary(status, contributingStations),
                    ContributingStations = contributingStations,
                    EvaluatedAt = DateTime.UtcNow,
                    AdministrativeArea = BuildAdministrativeAreaInfo(ward, wardParentArea),
                    GeoJson = ParseGeoJson(ward.Geometry)
                }
            };
        }

        // District: Get all child wards, then get all stations from those wards
        private async Task<AdministrativeAreasEvaluateResponse> EvaluateDistrictAsync(AdministrativeArea district, CancellationToken ct)
        {
            // Get parent area once
            AdministrativeArea? districtParentArea = district.ParentId.HasValue 
                ? await _administrativeAreaRepository.GetByIdAsync(district.ParentId.Value, ct) 
                : null;

            // 1. Get all child wards
            var (childWards, _) = await _administrativeAreaRepository.GetAdministrativeAreasAsync(
                searchTerm: null,
                level: "ward",
                parentId: district.Id,
                pageNumber: 1,
                pageSize: 1000,
                ct);

            var wardIds = childWards.Select(w => w.Id).ToList();

            if (!wardIds.Any())
            {

                return new AdministrativeAreasEvaluateResponse
                {
                    Success = true,
                    Message = "No wards found in this district",
                    StatusCode = AreaStatusCode.Success,
                    Data = new AdministrativeAreaStatusDto
                    {
                        AdministrativeAreaId = district.Id,
                        Status = "Unknown",
                        SeverityLevel = -1,
                        Summary = "No wards found in this district.",
                        EvaluatedAt = DateTime.UtcNow,
                        AdministrativeArea = BuildAdministrativeAreaInfo(district, districtParentArea),
                        GeoJson = ParseGeoJson(district.Geometry)
                    }
                };
            }

            // 2. Get all active stations
            var (allStations, _) = await _stationRepository.GetStationsAsync(
                searchTerm: null,
                status: "active",
                pageNumber: 1,
                pageSize: 10000,
                ct);

            // 3. Get stations from all child wards
            var stationsInDistrict = allStations
                .Where(s => s.AdministrativeAreaId.HasValue && wardIds.Contains(s.AdministrativeAreaId.Value))
                .ToList();

            if (!stationsInDistrict.Any())
            {

                return new AdministrativeAreasEvaluateResponse
                {
                    Success = true,
                    Message = "No sensors found in this district",
                    StatusCode = AreaStatusCode.Success,
                    Data = new AdministrativeAreaStatusDto
                    {
                        AdministrativeAreaId = district.Id,
                        Status = "Unknown",
                        SeverityLevel = -1,
                        Summary = "No sensors found in any ward within this district.",
                        EvaluatedAt = DateTime.UtcNow,
                        AdministrativeArea = BuildAdministrativeAreaInfo(district, districtParentArea),
                        GeoJson = ParseGeoJson(district.Geometry)
                    }
                };
            }

            // 4. Create ward lookup dictionary for quick access
            var wardLookup = childWards.ToDictionary(w => w.Id, w => w);

            // 5. Get latest readings
            var stationIds = stationsInDistrict.Select(s => s.Id).ToList();
            var latestReadings = await _sensorReadingRepository.GetLatestReadingsByStationsAsync(stationIds, ct);

            // 6. Aggregate status with ward information
            var contributingStations = stationsInDistrict.Select(station =>
            {
                var reading = latestReadings.FirstOrDefault(r => r.StationId == station.Id);
                var (severity, weight) = CalculateSeverity(reading?.Value ?? 0, station);

                // Get ward information
                ContributingWardInfoDto? wardInfo = null;
                if (station.AdministrativeAreaId.HasValue && wardLookup.TryGetValue(station.AdministrativeAreaId.Value, out var ward))
                {
                    wardInfo = new ContributingWardInfoDto
                    {
                        Id = ward.Id,
                        Name = ward.Name,
                        Code = ward.Code
                    };
                }

                return new ContributingStationDto
                {
                    StationId = station.Id,
                    StationCode = station.Code,
                    Distance = 0,
                    WaterLevel = reading?.Value ?? 0,
                    Severity = severity,
                    Weight = weight,
                    Ward = wardInfo
                };
            }).ToList();

            var maxWeight = contributingStations.Any() ? contributingStations.Max(s => s.Weight) : -1;
            var status = MapWeightToStatus(maxWeight);

            return new AdministrativeAreasEvaluateResponse
            {
                Success = true,
                Message = "District status evaluated successfully",
                StatusCode = AreaStatusCode.Success,
                Data = new AdministrativeAreaStatusDto
                {
                    AdministrativeAreaId = district.Id,
                    Status = status,
                    SeverityLevel = maxWeight,
                    Summary = BuildSummary(status, contributingStations),
                    ContributingStations = contributingStations,
                    EvaluatedAt = DateTime.UtcNow,
                    AdministrativeArea = BuildAdministrativeAreaInfo(district, districtParentArea),
                    GeoJson = ParseGeoJson(district.Geometry)
                }
            };
        }

        // City: Get all districts, then wards, then highest reading per ward
        private async Task<AdministrativeAreasEvaluateResponse> EvaluateCityAsync(AdministrativeArea city, CancellationToken ct)
        {
            // 1. Get all child districts
            var (districts, _) = await _administrativeAreaRepository.GetAdministrativeAreasAsync(
                searchTerm: null,
                level: "district",
                parentId: city.Id,
                pageNumber: 1,
                pageSize: 1000,
                ct);

            var districtIds = districts.Select(d => d.Id).ToList();

            if (!districtIds.Any())
            {
                return new AdministrativeAreasEvaluateResponse
                {
                    Success = true,
                    Message = "No districts found in this city",
                    StatusCode = AreaStatusCode.Success,
                    Data = new AdministrativeAreaStatusDto
                    {
                        AdministrativeAreaId = city.Id,
                        Status = "Unknown",
                        SeverityLevel = -1,
                        Summary = "No districts found in this city.",
                        EvaluatedAt = DateTime.UtcNow,
                        AdministrativeArea = BuildAdministrativeAreaInfo(city, null),
                        GeoJson = ParseGeoJson(city.Geometry)
                    }
                };
            }

            // 2. Get all child wards from all districts with district mapping
            var allWardIds = new List<Guid>();
            var wardToDistrictMap = new Dictionary<Guid, AdministrativeArea>(); // wardId -> district
            var allWards = new List<AdministrativeArea>();
            
            foreach (var district in districts)
            {
                var (wards, _) = await _administrativeAreaRepository.GetAdministrativeAreasAsync(
                    searchTerm: null,
                    level: "ward",
                    parentId: district.Id,
                    pageNumber: 1,
                    pageSize: 1000,
                    ct);
                
                foreach (var ward in wards)
                {
                    allWardIds.Add(ward.Id);
                    wardToDistrictMap[ward.Id] = district;
                    allWards.Add(ward);
                }
            }
            
            // Create ward lookup dictionary
            var wardLookup = allWards.ToDictionary(w => w.Id, w => w);

            if (!allWardIds.Any())
            {
                return new AdministrativeAreasEvaluateResponse
                {
                    Success = true,
                    Message = "No wards found in this city",
                    StatusCode = AreaStatusCode.Success,
                    Data = new AdministrativeAreaStatusDto
                    {
                        AdministrativeAreaId = city.Id,
                        Status = "Unknown",
                        SeverityLevel = -1,
                        Summary = "No wards found in any district within this city.",
                        EvaluatedAt = DateTime.UtcNow,
                        AdministrativeArea = BuildAdministrativeAreaInfo(city, null),
                        GeoJson = ParseGeoJson(city.Geometry)
                    }
                };
            }

            // 3. Get all active stations
            var (allStations, _) = await _stationRepository.GetStationsAsync(
                searchTerm: null,
                status: "active",
                pageNumber: 1,
                pageSize: 10000,
                ct);

            // 4. Group stations by ward and get highest reading per ward
            var contributingStations = new List<ContributingStationDto>();
            
            foreach (var wardId in allWardIds)
            {
                var stationsInWard = allStations
                    .Where(s => s.AdministrativeAreaId == wardId)
                    .ToList();

                if (!stationsInWard.Any())
                    continue;

                // Get latest readings for this ward's stations
                var stationIds = stationsInWard.Select(s => s.Id).ToList();
                var latestReadings = await _sensorReadingRepository.GetLatestReadingsByStationsAsync(stationIds, ct);

                if (!latestReadings.Any())
                    continue;

                // Find highest and newest reading in this ward
                var highestReading = latestReadings
                    .OrderByDescending(r => r.Value)
                    .ThenByDescending(r => r.MeasuredAt)
                    .First();

                var station = stationsInWard.First(s => s.Id == highestReading.StationId);
                var (severity, weight) = CalculateSeverity(highestReading.Value, station);

                // Get ward and district information
                ContributingWardInfoDto? wardInfo = null;
                ContributingDistrictInfoDto? districtInfo = null;
                
                if (wardLookup.TryGetValue(wardId, out var ward))
                {
                    wardInfo = new ContributingWardInfoDto
                    {
                        Id = ward.Id,
                        Name = ward.Name,
                        Code = ward.Code
                    };
                    
                    // Get district information
                    if (wardToDistrictMap.TryGetValue(wardId, out var district))
                    {
                        districtInfo = new ContributingDistrictInfoDto
                        {
                            Id = district.Id,
                            Name = district.Name,
                            Code = district.Code
                        };
                    }
                }

                contributingStations.Add(new ContributingStationDto
                {
                    StationId = station.Id,
                    StationCode = station.Code,
                    Distance = 0,
                    WaterLevel = highestReading.Value,
                    Severity = severity,
                    Weight = weight,
                    Ward = wardInfo,
                    District = districtInfo
                });
            }

            if (!contributingStations.Any())
            {
                return new AdministrativeAreasEvaluateResponse
                {
                    Success = true,
                    Message = "No sensor readings found in this city",
                    StatusCode = AreaStatusCode.Success,
                    Data = new AdministrativeAreaStatusDto
                    {
                        AdministrativeAreaId = city.Id,
                        Status = "Unknown",
                        SeverityLevel = -1,
                        Summary = "No sensor readings found in any ward within this city.",
                        EvaluatedAt = DateTime.UtcNow,
                        AdministrativeArea = BuildAdministrativeAreaInfo(city, null),
                        GeoJson = ParseGeoJson(city.Geometry)
                    }
                };
            }

            // 5. Aggregate status from highest readings per ward
            var maxWeight = contributingStations.Max(s => s.Weight);
            var status = MapWeightToStatus(maxWeight);

            return new AdministrativeAreasEvaluateResponse
            {
                Success = true,
                Message = "City status evaluated successfully",
                StatusCode = AreaStatusCode.Success,
                Data = new AdministrativeAreaStatusDto
                {
                    AdministrativeAreaId = city.Id,
                    Status = status,
                    SeverityLevel = maxWeight,
                    Summary = BuildSummary(status, contributingStations),
                    ContributingStations = contributingStations,
                    EvaluatedAt = DateTime.UtcNow,
                    AdministrativeArea = BuildAdministrativeAreaInfo(city, null),
                    GeoJson = ParseGeoJson(city.Geometry)
                }
            };
        }
    }
}

