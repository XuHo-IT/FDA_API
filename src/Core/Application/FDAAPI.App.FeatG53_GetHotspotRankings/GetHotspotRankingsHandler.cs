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

namespace FDAAPI.App.FeatG53_GetHotspotRankings
{
    public class GetHotspotRankingsHandler : IRequestHandler<GetHotspotRankingsRequest, GetHotspotRankingsResponse>
    {
        private readonly IFloodAnalyticsHotspotRepository _hotspotRepository;
        private readonly IAdministrativeAreaRepository _administrativeAreaRepository;
        private readonly IDistributedCache? _cache;

        public GetHotspotRankingsHandler(
            IFloodAnalyticsHotspotRepository hotspotRepository,
            IAdministrativeAreaRepository administrativeAreaRepository,
            IDistributedCache? cache = null)
        {
            _hotspotRepository = hotspotRepository;
            _administrativeAreaRepository = administrativeAreaRepository;
            _cache = cache;
        }

        public async Task<GetHotspotRankingsResponse> Handle(
            GetHotspotRankingsRequest request,
            CancellationToken ct)
        {
            try
            {
                // Set default period (last 30 days)
                var periodEnd = request.PeriodEnd.HasValue
                    ? ToUtc(request.PeriodEnd.Value)
                    : DateTime.UtcNow;

                var periodStart = request.PeriodStart.HasValue
                    ? ToUtc(request.PeriodStart.Value)
                    : periodEnd.AddDays(-30);
                var topN = request.TopN ?? 20;

                // Try cache first
                var cacheKey = $"analytics:hotspots:{periodStart:yyyyMMdd}:{periodEnd:yyyyMMdd}:{topN}";
                var cachedData = await GetFromCacheAsync<HotspotRankingsDto>(cacheKey, ct);

                if (cachedData != null)
                {
                    return new GetHotspotRankingsResponse
                    {
                        Success = true,
                        Message = "Hotspot rankings retrieved successfully (cached)",
                        StatusCode = AnalyticsStatusCode.Success,
                        Data = cachedData
                    };
                }

                // Get hotspots
                var hotspots = await _hotspotRepository.GetTopHotspotsAsync(
                    periodStart,
                    periodEnd,
                    topN,
                    ct);

                // Get area info for each hotspot
                var areaIds = hotspots.Select(h => h.AdministrativeAreaId).Distinct().ToList();
                var areas = await _administrativeAreaRepository.GetByIdsAsync(areaIds, ct);
                var areaDict = areas.ToDictionary(a => a.Id, a => a.Name);

                var hotspotDtos = hotspots.Select(h => new HotspotDto
                {
                    AdministrativeAreaId = h.AdministrativeAreaId,
                    AdministrativeAreaName = areaDict.GetValueOrDefault(h.AdministrativeAreaId, "Unknown"),
                    Score = h.Score,
                    Rank = h.Rank ?? 0,
                    CalculatedAt = h.CalculatedAt
                }).ToList();

                var result = new HotspotRankingsDto
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    AreaLevel = request.AreaLevel ?? "district",
                    Hotspots = hotspotDtos
                };

                // Cache result only if there are hotspots (best practice: don't cache empty results)
                if (hotspotDtos.Any())
                {
                    await SetCacheAsync(cacheKey, result, TimeSpan.FromHours(6), ct);
                }

                return new GetHotspotRankingsResponse
                {
                    Success = true,
                    Message = "Hotspot rankings retrieved successfully",
                    StatusCode = AnalyticsStatusCode.Success,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new GetHotspotRankingsResponse
                {
                    Success = false,
                    Message = $"Error retrieving hotspot rankings: {ex.Message}",
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

