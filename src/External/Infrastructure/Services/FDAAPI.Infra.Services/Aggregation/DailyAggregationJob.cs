using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FDAAPI.Infra.Services.Aggregation
{
    /// <summary>
    /// Background job that aggregates hourly data into daily summaries.
    /// Runs daily at 00:15 UTC to ensure all hourly aggregates from the previous day are available.
    /// </summary>
    [DisallowConcurrentExecution]
    public class DailyAggregationJob : IJob
    {
        private readonly ISensorHourlyAggRepository _hourlyAggRepository;
        private readonly ISensorDailyAggRepository _dailyAggRepository;
        private readonly IStationRepository _stationRepository;
        private readonly ILogger<DailyAggregationJob> _logger;

        // Severity thresholds in centimeters
        private const decimal CAUTION_THRESHOLD = 100m;
        private const decimal WARNING_THRESHOLD = 200m;
        private const decimal CRITICAL_THRESHOLD = 300m;

        public DailyAggregationJob(
            ISensorHourlyAggRepository hourlyAggRepository,
            ISensorDailyAggRepository dailyAggRepository,
            IStationRepository stationRepository,
            ILogger<DailyAggregationJob> logger)
        {
            _hourlyAggRepository = hourlyAggRepository;
            _dailyAggRepository = dailyAggRepository;
            _stationRepository = stationRepository;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("DailyAggregationJob started at {StartTime}", startTime);

            try
            {
                // Calculate the date to aggregate (yesterday)
                var dateToAggregate = DateOnly.FromDateTime(startTime.AddDays(-1));

                _logger.LogInformation("Aggregating data for date: {Date}", dateToAggregate);

                // Get all active stations
                var stations = await _stationRepository.GetAllAsync(context.CancellationToken);
                var activeStations = stations.Where(s => s.Status == "active").ToList();

                _logger.LogInformation("Found {Count} active stations to aggregate", activeStations.Count);

                var aggregates = new List<SensorDailyAgg>();
                var processedCount = 0;
                var skippedCount = 0;

                foreach (var station in activeStations)
                {
                    try
                    {
                        // Check if aggregation already exists
                        var existing = await _dailyAggRepository.GetByStationAndDateAsync(
                            station.Id, dateToAggregate, context.CancellationToken);

                        if (existing != null)
                        {
                            _logger.LogDebug("Aggregation already exists for station {StationId} on {Date}",
                                station.Id, dateToAggregate);
                            skippedCount++;
                            continue;
                        }

                        // Get hourly aggregates for this day
                        var dayStart = dateToAggregate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                        var dayEnd = dateToAggregate.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc);

                        var hourlyAggs = await _hourlyAggRepository.GetByStationAndRangeAsync(
                            station.Id, dayStart, dayEnd, context.CancellationToken);

                        if (!hourlyAggs.Any())
                        {
                            _logger.LogDebug("No hourly aggregates found for station {StationId} on {Date}",
                                station.Id, dateToAggregate);
                            skippedCount++;
                            continue;
                        }

                        // Calculate daily aggregation
                        var aggregate = CreateDailyAggregate(station.Id, dateToAggregate, hourlyAggs);
                        aggregates.Add(aggregate);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error aggregating station {StationId} for date {Date}",
                            station.Id, dateToAggregate);
                    }
                }

                // Bulk insert aggregates
                if (aggregates.Any())
                {
                    await _dailyAggRepository.BulkInsertAsync(aggregates, context.CancellationToken);
                    _logger.LogInformation("Inserted {Count} daily aggregates", aggregates.Count);
                }

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "DailyAggregationJob completed. Processed: {Processed}, Skipped: {Skipped}, Duration: {Duration}ms",
                    processedCount, skippedCount, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DailyAggregationJob failed");
                throw;
            }
        }

        private SensorDailyAgg CreateDailyAggregate(Guid stationId, DateOnly date, List<SensorHourlyAgg> hourlyAggs)
        {
            // Calculate flood hours (hours where max level exceeded caution threshold)
            var floodHours = hourlyAggs.Count(h => h.MaxLevel >= CAUTION_THRESHOLD);

            // Determine peak severity
            var maxLevel = hourlyAggs.Max(h => h.MaxLevel);
            var peakSeverity = GetSeverityLevel(maxLevel);

            return new SensorDailyAgg
            {
                Id = Guid.NewGuid(),
                StationId = stationId,
                Date = date,
                MaxLevel = maxLevel,
                MinLevel = hourlyAggs.Min(h => h.MinLevel),
                AvgLevel = Math.Round(hourlyAggs.Average(h => h.AvgLevel), 2),
                RainfallTotal = null, // TODO: Integrate rainfall data if available
                ReadingCount = hourlyAggs.Sum(h => h.ReadingCount),
                FloodHours = floodHours,
                PeakSeverity = peakSeverity,
                CreatedAt = DateTime.UtcNow
            };
        }

        private int GetSeverityLevel(decimal maxLevel)
        {
            if (maxLevel >= CRITICAL_THRESHOLD) return 3; // Critical
            if (maxLevel >= WARNING_THRESHOLD) return 2;  // Warning
            if (maxLevel >= CAUTION_THRESHOLD) return 1;  // Caution
            return 0; // Safe
        }
    }
}
