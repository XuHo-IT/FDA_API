using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Analytics;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG52_GetSeverityAnalytics
{
    public class GetSeverityAnalyticsHandler : IRequestHandler<GetSeverityAnalyticsRequest, GetSeverityAnalyticsResponse>
    {
        private readonly IFloodAnalyticsSeverityRepository _severityRepository;
        private readonly IAdministrativeAreaRepository _administrativeAreaRepository;
        private readonly IDistributedCache? _cache;

        public GetSeverityAnalyticsHandler(
            IFloodAnalyticsSeverityRepository severityRepository,
            IAdministrativeAreaRepository administrativeAreaRepository,
            IDistributedCache? cache = null)
        {
            _severityRepository = severityRepository;
            _administrativeAreaRepository = administrativeAreaRepository;
            _cache = cache;
        }

        public async Task<GetSeverityAnalyticsResponse> Handle(
            GetSeverityAnalyticsRequest request,
            CancellationToken ct)
        {
            try
            {
                // Set default date range (last 30 days)
                var endDate = request.EndDate.HasValue
                    ? ToUtc(request.EndDate.Value)
                    : DateTime.UtcNow;

                var startDate = request.StartDate.HasValue
                    ? ToUtc(request.StartDate.Value)
                    : endDate.AddDays(-30);

                if (!request.AdministrativeAreaId.HasValue)
                {
                    return new GetSeverityAnalyticsResponse
                    {
                        Success = false,
                        Message = "Administrative area ID is required",
                        StatusCode = AnalyticsStatusCode.BadRequest
                    };
                }

                var areaId = request.AdministrativeAreaId.Value;

                // Try cache first
                var cacheKey = $"analytics:severity:{areaId}:{request.BucketType}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}";
                var cachedData = await GetFromCacheAsync<SeverityAnalyticsDto>(cacheKey, ct);

                if (cachedData != null)
                {
                    return new GetSeverityAnalyticsResponse
                    {
                        Success = true,
                        Message = "Severity analytics retrieved successfully (cached)",
                        StatusCode = AnalyticsStatusCode.Success,
                        Data = cachedData
                    };
                }

                // Get area info
                var area = await _administrativeAreaRepository.GetByIdAsync(areaId, ct);
                if (area == null)
                {
                    return new GetSeverityAnalyticsResponse
                    {
                        Success = false,
                        Message = "Administrative area not found",
                        StatusCode = AnalyticsStatusCode.NotFound
                    };
                }

                // Get severity data
                var severityData = await _severityRepository.GetByAdministrativeAreaAndPeriodAsync(
                    areaId,
                    startDate,
                    endDate,
                    request.BucketType,
                    ct);

                var dataPoints = severityData.Select(s => new SeverityDataPointDto
                {
                    TimeBucket = s.TimeBucket,
                    MaxLevel = s.MaxLevel,
                    AvgLevel = s.AvgLevel,
                    MinLevel = s.MinLevel,
                    DurationHours = s.DurationHours,
                    ReadingCount = s.ReadingCount,
                    CalculatedAt = s.CalculatedAt
                }).ToList();

                var result = new SeverityAnalyticsDto
                {
                    AdministrativeAreaId = areaId,
                    AdministrativeAreaName = area.Name,
                    BucketType = request.BucketType,
                    DataPoints = dataPoints
                };

                // Cache result only if there are data points (best practice: don't cache empty results)
                if (dataPoints.Any())
                {
                    await SetCacheAsync(cacheKey, result, TimeSpan.FromHours(1), ct);
                }

                return new GetSeverityAnalyticsResponse
                {
                    Success = true,
                    Message = "Severity analytics retrieved successfully",
                    StatusCode = AnalyticsStatusCode.Success,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new GetSeverityAnalyticsResponse
                {
                    Success = false,
                    Message = $"Error retrieving severity analytics: {ex.Message}",
                    StatusCode = AnalyticsStatusCode.InternalServerError
                };
            }
        }

        private async Task<T?> GetFromCacheAsync<T>(string key, CancellationToken ct) where T : class
        {
            if (_cache == null)
                return null;

            try
            {
                var cached = await _cache.GetStringAsync(key, ct);
                if (string.IsNullOrEmpty(cached))
                    return null;

                return JsonSerializer.Deserialize<T>(cached);
            }
            catch
            {
                return null;
            }
        }

        private async Task SetCacheAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct) where T : class
        {
            if (_cache == null)
                return;

            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                };

                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options, ct);
            }
            catch
            {
                // Ignore cache errors
            }
        }

        private static DateTime ToUtc(DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
                _ => dt
            };
        }
    }
}

