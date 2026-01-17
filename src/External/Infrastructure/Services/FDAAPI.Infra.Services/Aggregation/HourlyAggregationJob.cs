using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Domain.RelationalDb;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FDAAPI.Infra.Services.Aggregation
{
    /// <summary>
    /// Background job that aggregates sensor readings into hourly summaries.
    /// Runs every hour at :05 minutes to ensure all readings from the previous hour are captured.
    /// </summary>
    [DisallowConcurrentExecution]
    public class HourlyAggregationJob : IJob
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;
        private readonly ISensorHourlyAggRepository _hourlyAggRepository;
        private readonly IStationRepository _stationRepository;
        private readonly ILogger<HourlyAggregationJob> _logger;

        // Severity thresholds in centimeters
        private const decimal CAUTION_THRESHOLD = 100m;
        private const decimal WARNING_THRESHOLD = 200m;
        private const decimal CRITICAL_THRESHOLD = 300m;

        public HourlyAggregationJob(
            ISensorReadingRepository sensorReadingRepository,
            ISensorHourlyAggRepository hourlyAggRepository,
            IStationRepository stationRepository,
            ILogger<HourlyAggregationJob> logger)
        {
            _sensorReadingRepository = sensorReadingRepository;
            _hourlyAggRepository = hourlyAggRepository;
            _stationRepository = stationRepository;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("HourlyAggregationJob started at {StartTime}", startTime);

            try
            {
                // Calculate the hour to aggregate (previous hour)
                var currentHour = new DateTime(
                    startTime.Year, startTime.Month, startTime.Day,
                    startTime.Hour, 0, 0, DateTimeKind.Utc);
                var hourToAggregate = currentHour.AddHours(-1);

                _logger.LogInformation("Aggregating data for hour: {Hour}", hourToAggregate);

                // Get all active stations
                var stations = await _stationRepository.GetAllAsync(context.CancellationToken);
                var activeStations = stations.Where(s => s.Status == "active").ToList();

                _logger.LogInformation("Found {Count} active stations to aggregate", activeStations.Count);

                var aggregates = new List<SensorHourlyAgg>();
                var processedCount = 0;
                var skippedCount = 0;

                foreach (var station in activeStations)
                {
                    try
                    {
                        // Check if aggregation already exists
                        var existing = await _hourlyAggRepository.GetByStationAndHourAsync(
                            station.Id, hourToAggregate, context.CancellationToken);

                        if (existing != null)
                        {
                            _logger.LogDebug("Aggregation already exists for station {StationId} at {Hour}",
                                station.Id, hourToAggregate);
                            skippedCount++;
                            continue;
                        }

                        // Get readings for this hour
                        var readings = await _sensorReadingRepository.GetByStationAndTimeRangeAsync(
                            station.Id,
                            hourToAggregate,
                            hourToAggregate.AddHours(1).AddSeconds(-1),
                            10000, // Max readings per hour
                            context.CancellationToken);

                        if (!readings.Any())
                        {
                            _logger.LogDebug("No readings found for station {StationId} at {Hour}",
                                station.Id, hourToAggregate);
                            skippedCount++;
                            continue;
                        }

                        // Calculate aggregation
                        var aggregate = CreateHourlyAggregate(station.Id, hourToAggregate, readings);
                        aggregates.Add(aggregate);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error aggregating station {StationId} for hour {Hour}",
                            station.Id, hourToAggregate);
                    }
                }

                // Bulk insert aggregates
                if (aggregates.Any())
                {
                    await _hourlyAggRepository.BulkInsertAsync(aggregates, context.CancellationToken);
                    _logger.LogInformation("Inserted {Count} hourly aggregates", aggregates.Count);
                }

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "HourlyAggregationJob completed. Processed: {Processed}, Skipped: {Skipped}, Duration: {Duration}ms",
                    processedCount, skippedCount, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HourlyAggregationJob failed");
                throw;
            }
        }

        private SensorHourlyAgg CreateHourlyAggregate(Guid stationId, DateTime hourStart, List<SensorReading> readings)
        {
            var values = readings.Select(r => (decimal)r.Value).ToList();
            var expectedReadings = 12; // Assuming 5-minute intervals = 12 readings per hour

            return new SensorHourlyAgg
            {
                Id = Guid.NewGuid(),
                StationId = stationId,
                HourStart = hourStart,
                MaxLevel = values.Max(),
                MinLevel = values.Min(),
                AvgLevel = Math.Round(values.Average(), 2),
                ReadingCount = readings.Count,
                QualityScore = Math.Round((decimal)readings.Count / expectedReadings * 100, 2),
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
