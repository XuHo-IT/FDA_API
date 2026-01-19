using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG49_HotspotAggregation
{
    public class HotspotAggregationBackgroundJob
    {
        private readonly IFloodAnalyticsFrequencyRepository _frequencyRepository;
        private readonly IFloodAnalyticsSeverityRepository _severityRepository;
        private readonly IFloodAnalyticsHotspotRepository _hotspotRepository;
        private readonly IAnalyticsJobRunRepository _jobRunRepository;
        private readonly IAdministrativeAreaRepository _administrativeAreaRepository;
        private readonly IDistributedCache? _cache;

        public HotspotAggregationBackgroundJob(
            IFloodAnalyticsFrequencyRepository frequencyRepository,
            IFloodAnalyticsSeverityRepository severityRepository,
            IFloodAnalyticsHotspotRepository hotspotRepository,
            IAnalyticsJobRunRepository jobRunRepository,
            IAdministrativeAreaRepository administrativeAreaRepository,
            IDistributedCache? cache = null)
        {
            _frequencyRepository = frequencyRepository;
            _severityRepository = severityRepository;
            _hotspotRepository = hotspotRepository;
            _jobRunRepository = jobRunRepository;
            _administrativeAreaRepository = administrativeAreaRepository;
            _cache = cache;
        }

        public async Task ExecuteAsync(
            DateTime periodStart,
            DateTime periodEnd,
            int? topN,
            Guid jobRunId)
        {
            var startTime = DateTime.UtcNow;
            var recordsProcessed = 0;
            var recordsCreated = 0;
            var ct = CancellationToken.None; // Hangfire cannot serialize CancellationToken, so use None internally

            try
            {
                // 1. Get all administrative areas for hotspot calculation
                // Since frequency/severity data is typically at ward level, prioritize ward first
                // Then try district, then all areas
                var areas = await _administrativeAreaRepository.GetByLevelAsync("ward", ct);
                if (!areas.Any())
                {
                    areas = await _administrativeAreaRepository.GetByLevelAsync("district", ct);
                }
                if (!areas.Any())
                {
                    // Fallback: get all areas if no ward/district found
                    areas = await _administrativeAreaRepository.GetAllAsync(ct);
                }

                // If still no areas, there's nothing to process
                if (!areas.Any())
                {
                    await _jobRunRepository.UpdateStatusAsync(
                        jobRunId,
                        "SUCCESS",
                        "No administrative areas found",
                        DateTime.UtcNow,
                        (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                        0,
                        0,
                        ct);
                    return;
                }

                // 2. Get frequency and severity data for the period
                // Use daily bucket type for hotspot calculation
                var hotspots = new List<FloodAnalyticsHotspot>();

                foreach (var area in areas)
                {
                    recordsProcessed++;

                    // Get frequency data (sum of event_count and exceed_count for the period)
                    // Query uses: TimeBucket >= startDate AND TimeBucket < endDate
                    // So we add 1 day to endDate to include the last day's data
                    var queryEndDate = periodEnd.Date.AddDays(1); // Set to start of next day to include last day
                    var frequencyData = await _frequencyRepository.GetByAdministrativeAreaAndPeriodAsync(
                        area.Id,
                        periodStart,
                        queryEndDate,
                        "day",
                        ct);

                    // Get severity data (max of max_level, avg of avg_level for the period)
                    var severityData = await _severityRepository.GetByAdministrativeAreaAndPeriodAsync(
                        area.Id,
                        periodStart,
                        queryEndDate,
                        "day",
                        ct);

                    // Debug: Check if we have any data
                    if (!frequencyData.Any() && !severityData.Any())
                    {
                        // Skip areas with no data - this is expected for areas without flood events
                        continue;
                    }

                    // Calculate hotspot score
                    var totalEventCount = frequencyData.Sum(f => f.EventCount);
                    var totalExceedCount = frequencyData.Sum(f => f.ExceedCount);
                    var maxSeverity = severityData.Any() ? severityData.Max(s => s.MaxLevel ?? 0) : 0;
                    var avgSeverity = severityData.Any() ? severityData.Average(s => s.AvgLevel ?? 0) : 0;
                    var totalDurationHours = severityData.Sum(s => s.DurationHours);

                    // Score formula: (frequency * 0.4) + (severity * 0.35) + (duration * 0.25)
                    // Normalize values (assuming max values for normalization)
                    const double maxEventCount = 100; // Adjust based on domain knowledge
                    const double maxSeverityLevel = 5.0; // meters
                    const double maxDurationHours = 720; // 30 days * 24 hours

                    var frequencyScore = Math.Min((totalEventCount / maxEventCount) * 100 * 0.4, 40);
                    var severityScore = Math.Min((double)(maxSeverity / (decimal)maxSeverityLevel) * 100 * 0.35, 35);
                    var durationScore = Math.Min((totalDurationHours / maxDurationHours) * 100 * 0.25, 25);

                    var totalScore = (decimal)(frequencyScore + severityScore + durationScore);

                    var hotspot = new FloodAnalyticsHotspot
                    {
                        Id = Guid.NewGuid(),
                        AdministrativeAreaId = area.Id,
                        Score = totalScore,
                        Rank = null, // Will be set after sorting
                        PeriodStart = periodStart,
                        PeriodEnd = periodEnd,
                        CalculatedAt = DateTime.UtcNow
                    };

                    hotspots.Add(hotspot);
                }

                // 3. Rank hotspots by score
                var rankedHotspots = hotspots
                    .OrderByDescending(h => h.Score)
                    .Select((h, index) =>
                    {
                        h.Rank = index + 1;
                        return h;
                    })
                    .ToList();

                // 4. Apply TopN filter if specified
                if (topN.HasValue && topN.Value > 0)
                {
                    rankedHotspots = rankedHotspots.Take(topN.Value).ToList();
                }

                // 5. Bulk upsert hotspots
                if (rankedHotspots.Any())
                {
                    await _hotspotRepository.BulkUpsertAsync(rankedHotspots, ct);
                    recordsCreated = rankedHotspots.Count;
                }

                // 6. Invalidate cache for hotspot rankings
                await InvalidateHotspotCacheAsync(periodStart, periodEnd, topN, ct);

                // 7. Update job run status
                var executionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                await _jobRunRepository.UpdateStatusAsync(
                    jobRunId,
                    "SUCCESS",
                    null,
                    DateTime.UtcNow,
                    executionTime,
                    recordsProcessed,
                    recordsCreated,
                    ct);
            }
            catch (Exception ex)
            {
                var executionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                await _jobRunRepository.UpdateStatusAsync(
                    jobRunId,
                    "FAILED",
                    ex.Message,
                    DateTime.UtcNow,
                    executionTime,
                    recordsProcessed,
                    recordsCreated,
                    ct);
                throw;
            }
        }

        private async Task InvalidateHotspotCacheAsync(
            DateTime periodStart,
            DateTime periodEnd,
            int? topN,
            CancellationToken ct)
        {
            if (_cache == null)
                return;

            try
            {
                // Invalidate cache for hotspot rankings
                // Cache key format: analytics:hotspots:{periodStart:yyyyMMdd}:{periodEnd:yyyyMMdd}:{topN}
                var periodStartStr = periodStart.ToString("yyyyMMdd");
                var periodEndStr = periodEnd.ToString("yyyyMMdd");

                // Invalidate for the specific topN and common values (20, 50, 100) to cover most cases
                var topNValues = new List<int?> { topN, 20, 50, 100 }.Distinct().ToList();

                foreach (var n in topNValues)
                {
                    if (n.HasValue)
                    {
                        var cacheKey = $"analytics:hotspots:{periodStartStr}:{periodEndStr}:{n.Value}";
                        await _cache.RemoveAsync(cacheKey, ct);
                    }
                }
            }
            catch
            {
                // Ignore cache invalidation errors - don't fail the job if cache invalidation fails
            }
        }
    }
}

