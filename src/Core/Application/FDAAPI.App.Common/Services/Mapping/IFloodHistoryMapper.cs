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
    public interface IFloodHistoryMapper
    {
        FloodDataPointDto MapToDataPoint(SensorReading reading);
        FloodDataPointDto MapToDataPoint(SensorHourlyAgg hourlyAgg);
        FloodDataPointDto MapToDataPoint(SensorDailyAgg dailyAgg);

        List<FloodDataPointDto> MapToDataPoints(IEnumerable<SensorReading> readings);
        List<FloodDataPointDto> MapToDataPoints(IEnumerable<SensorHourlyAgg> hourlyAggs);
        List<FloodDataPointDto> MapToDataPoints(IEnumerable<SensorDailyAgg> dailyAggs);

        FloodTrendDataPointDto MapToTrendDataPoint(SensorDailyAgg dailyAgg);
        List<FloodTrendDataPointDto> MapToTrendDataPoints(IEnumerable<SensorDailyAgg> dailyAggs);

        (string severity, int level) CalculateSeverity(double waterLevelCm);
    }
}
