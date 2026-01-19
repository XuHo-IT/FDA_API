using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG47_FrequencyAggregation
{
    public class FrequencyAggregationBackgroundJob
    {
        private readonly IFloodEventRepository _floodEventRepository;
        private readonly ISensorReadingRepository _sensorReadingRepository;
        private readonly IFloodAnalyticsFrequencyRepository _frequencyRepository;
        private readonly IAnalyticsJobRunRepository _jobRunRepository;
        private readonly IAdministrativeAreaRepository _administrativeAreaRepository;
        private readonly IDistributedCache? _cache;

        public FrequencyAggregationBackgroundJob(
            IFloodEventRepository floodEventRepository,
            ISensorReadingRepository sensorReadingRepository,
            IFloodAnalyticsFrequencyRepository frequencyRepository,
            IAnalyticsJobRunRepository jobRunRepository,
            IAdministrativeAreaRepository administrativeAreaRepository,
            IDistributedCache? cache = null)
        {
            _floodEventRepository = floodEventRepository;
            _sensorReadingRepository = sensorReadingRepository;
            _frequencyRepository = frequencyRepository;
            _jobRunRepository = jobRunRepository;
            _administrativeAreaRepository = administrativeAreaRepository;
            _cache = cache;
        }

        public async Task ExecuteAsync(
            string bucketType,
            DateTime startDate,
            DateTime endDate,
            List<Guid>? administrativeAreaIds,
            Guid jobRunId)
        {
            var startTime = DateTime.UtcNow;
            var recordsProcessed = 0;
            var recordsCreated = 0;
            var ct = CancellationToken.None; // Hangfire cannot serialize CancellationToken, so use None internally

            try
            {
                // 1. Get administrative areas to process
                List<AdministrativeArea> areas;
                if (administrativeAreaIds != null && administrativeAreaIds.Any())
                {
                    areas = await _administrativeAreaRepository.GetByIdsAsync(administrativeAreaIds, ct);
                }
                else
                {
                    areas = await _administrativeAreaRepository.GetByLevelAsync("ward", ct);
                    if (!areas.Any())
                    {
                        areas = await _administrativeAreaRepository.GetByLevelAsync("district", ct);
                    }
                }

                // 2. Generate time buckets
                var buckets = GenerateTimeBuckets(startDate, endDate, bucketType);

                // 3. Process each bucket
                var aggregates = new List<FloodAnalyticsFrequency>();

                foreach (var bucket in buckets)
                {
                    foreach (var area in areas)
                    {
                        recordsProcessed++;

                        // Count flood events
                        var eventCount = await _floodEventRepository.CountByAdministrativeAreaAndPeriodAsync(
                            area.Id,
                            bucket.Start,
                            bucket.End,
                            ct);

                        // Count exceedances (readings with severity >= 2)
                        var exceedCount = await _sensorReadingRepository.CountExceedancesByAdministrativeAreaAndPeriodAsync(
                            area.Id,
                            bucket.Start,
                            bucket.End,
                            ct);

                        // Create aggregate
                        var aggregate = new FloodAnalyticsFrequency
                        {
                            Id = Guid.NewGuid(),
                            AdministrativeAreaId = area.Id,
                            TimeBucket = bucket.Start,
                            BucketType = bucketType,
                            EventCount = eventCount,
                            ExceedCount = exceedCount,
                            CalculatedAt = DateTime.UtcNow
                        };

                        aggregates.Add(aggregate);
                    }
                }

                // 4. Bulk upsert aggregates
                if (aggregates.Any())
                {
                    await _frequencyRepository.BulkUpsertAsync(aggregates, ct);
                    recordsCreated = aggregates.Count;
                }

                // 5. Invalidate cache for affected areas and date ranges
                await InvalidateCacheForAreasAsync(areas, bucketType, startDate, endDate, ct);

                // 6. Update job run status
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

        private List<TimeBucket> GenerateTimeBuckets(DateTime startDate, DateTime endDate, string bucketType)
        {
            var buckets = new List<TimeBucket>();
            var current = TruncateToBucket(startDate, bucketType);

            while (current < endDate)
            {
                var bucketEnd = GetBucketEnd(current, bucketType);
                if (bucketEnd > endDate)
                    bucketEnd = endDate;

                buckets.Add(new TimeBucket
                {
                    Start = current,
                    End = bucketEnd
                });

                current = bucketEnd;
            }

            return buckets;
        }

        private DateTime TruncateToBucket(DateTime date, string bucketType)
        {
            return bucketType.ToLower() switch
            {
                "day" => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc),
                "week" => date.AddDays(-(int)date.DayOfWeek).Date,
                "month" => new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                "year" => new DateTime(date.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                _ => date
            };
        }

        private DateTime GetBucketEnd(DateTime bucketStart, string bucketType)
        {
            return bucketType.ToLower() switch
            {
                "day" => bucketStart.AddDays(1),
                "week" => bucketStart.AddDays(7),
                "month" => bucketStart.AddMonths(1),
                "year" => bucketStart.AddYears(1),
                _ => bucketStart.AddDays(1)
            };
        }

        private class TimeBucket
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        private async Task InvalidateCacheForAreasAsync(
            List<AdministrativeArea> areas,
            string bucketType,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct)
        {
            if (_cache == null)
                return;

            try
            {
                // Invalidate cache for each area and date range combination
                // Cache key format: analytics:frequency:{areaId}:{bucketType}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}
                var startDateStr = startDate.ToString("yyyyMMdd");
                var endDateStr = endDate.ToString("yyyyMMdd");

                foreach (var area in areas)
                {
                    var cacheKey = $"analytics:frequency:{area.Id}:{bucketType}:{startDateStr}:{endDateStr}";
                    await _cache.RemoveAsync(cacheKey, ct);
                }
            }
            catch
            {
                // Ignore cache invalidation errors - don't fail the job if cache invalidation fails
            }
        }
    }
}

