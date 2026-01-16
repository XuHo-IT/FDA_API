using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb;
using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.Common.Services.Mapping
{
    public class FloodHistoryMapper : IFloodHistoryMapper
    {
        // Severity thresholds in centimeters
        private const double CAUTION_THRESHOLD_CM = 100.0;   // 1.0m
        private const double WARNING_THRESHOLD_CM = 200.0;   // 2.0m
        private const double CRITICAL_THRESHOLD_CM = 300.0;  // 3.0m

        public FloodDataPointDto MapToDataPoint(SensorReading reading)
        {
            var (severity, level) = CalculateSeverity(reading.Value);

            return new FloodDataPointDto
            {
                Timestamp = reading.MeasuredAt,
                Value = reading.Value,
                ValueMeters = reading.Value / 100.0,
                QualityFlag = "ok", // TODO: Map from reading.Status
                Severity = severity,
                SeverityLevel = level
            };
        }

        public FloodDataPointDto MapToDataPoint(SensorHourlyAgg hourlyAgg)
        {
            var avgValue = (double)hourlyAgg.AvgLevel;
            var (severity, level) = CalculateSeverity((double)hourlyAgg.MaxLevel);

            return new FloodDataPointDto
            {
                Timestamp = hourlyAgg.HourStart,
                Value = avgValue,
                ValueMeters = avgValue / 100.0,
                QualityFlag = hourlyAgg.QualityScore >= 90 ? "ok" :
                              hourlyAgg.QualityScore >= 70 ? "suspect" : "bad",
                Severity = severity,
                SeverityLevel = level
            };
        }

        public FloodDataPointDto MapToDataPoint(SensorDailyAgg dailyAgg)
        {
            var avgValue = (double)dailyAgg.AvgLevel;
            var (severity, level) = CalculateSeverity((double)dailyAgg.MaxLevel);

            return new FloodDataPointDto
            {
                Timestamp = dailyAgg.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                Value = avgValue,
                ValueMeters = avgValue / 100.0,
                QualityFlag = "ok",
                Severity = severity,
                SeverityLevel = level
            };
        }

        public List<FloodDataPointDto> MapToDataPoints(IEnumerable<SensorReading> readings)
        {
            return readings.Select(MapToDataPoint).ToList();
        }

        public List<FloodDataPointDto> MapToDataPoints(IEnumerable<SensorHourlyAgg> hourlyAggs)
        {
            return hourlyAggs.Select(MapToDataPoint).ToList();
        }

        public List<FloodDataPointDto> MapToDataPoints(IEnumerable<SensorDailyAgg> dailyAggs)
        {
            return dailyAggs.Select(MapToDataPoint).ToList();
        }

        public FloodTrendDataPointDto MapToTrendDataPoint(SensorDailyAgg dailyAgg)
        {
            var (severity, _) = CalculateSeverity((double)dailyAgg.MaxLevel);

            return new FloodTrendDataPointDto
            {
                Period = dailyAgg.Date.ToString("yyyy-MM-dd"),
                PeriodStart = dailyAgg.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                PeriodEnd = dailyAgg.Date.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc),
                MaxLevel = (double)dailyAgg.MaxLevel,
                MinLevel = (double)dailyAgg.MinLevel,
                AvgLevel = (double)dailyAgg.AvgLevel,
                ReadingCount = dailyAgg.ReadingCount,
                FloodHours = dailyAgg.FloodHours,
                RainfallTotal = dailyAgg.RainfallTotal.HasValue ? (double?)dailyAgg.RainfallTotal.Value : null,
                PeakSeverity = severity
            };
        }

        public List<FloodTrendDataPointDto> MapToTrendDataPoints(IEnumerable<SensorDailyAgg> dailyAggs)
        {
            return dailyAggs.Select(MapToTrendDataPoint).ToList();
        }

        public (string severity, int level) CalculateSeverity(double waterLevelCm)
        {
            if (waterLevelCm >= CRITICAL_THRESHOLD_CM)
                return ("critical", 3);
            if (waterLevelCm >= WARNING_THRESHOLD_CM)
                return ("warning", 2);
            if (waterLevelCm >= CAUTION_THRESHOLD_CM)
                return ("caution", 1);
            return ("safe", 0);
        }
    }
}
