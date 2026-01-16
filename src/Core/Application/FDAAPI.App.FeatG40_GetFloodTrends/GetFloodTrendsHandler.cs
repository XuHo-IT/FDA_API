using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.FloodHistory;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG40_GetFloodTrends
{
    public class GetFloodTrendsHandler : IRequestHandler<GetFloodTrendsRequest, GetFloodTrendsResponse>
    {
        private readonly ISensorDailyAggRepository _dailyAggRepository;
        private readonly IStationRepository _stationRepository;
        private readonly IFloodHistoryMapper _mapper;

        public GetFloodTrendsHandler(
            ISensorDailyAggRepository dailyAggRepository,
            IStationRepository stationRepository,
            IFloodHistoryMapper mapper)
        {
            _dailyAggRepository = dailyAggRepository;
            _stationRepository = stationRepository;
            _mapper = mapper;
        }

        public async Task<GetFloodTrendsResponse> Handle(
            GetFloodTrendsRequest request,
            CancellationToken ct)
        {
            try
            {
                // Get station info
                var station = await _stationRepository.GetByIdAsync(request.StationId, ct);
                if (station == null)
                {
                    return new GetFloodTrendsResponse
                    {
                        Success = false,
                        Message = "Station not found",
                        StatusCode = FloodHistoryStatusCode.NotFound
                    };
                }

                // Calculate date range based on period
                var (startDate, endDate) = CalculateDateRange(request.Period, request.StartDate, request.EndDate);

                // Get daily aggregates
                var dailyAggs = await _dailyAggRepository.GetByStationAndRangeAsync(
                    request.StationId,
                    DateOnly.FromDateTime(startDate),
                    DateOnly.FromDateTime(endDate),
                    ct);

                if (!dailyAggs.Any())
                {
                    return new GetFloodTrendsResponse
                    {
                        Success = true,
                        Message = "No data available for the specified period",
                        StatusCode = FloodHistoryStatusCode.Success,
                        Data = new FloodTrendDto
                        {
                            StationId = station.Id,
                            StationName = station.Name,
                            StationCode = station.Code,
                            Period = request.Period,
                            Granularity = request.Granularity,
                            DataPoints = new List<FloodTrendDataPointDto>(),
                            Summary = new FloodTrendSummaryDto()
                        }
                    };
                }

                // Map to trend data points based on granularity
                var dataPoints = request.Granularity.ToLower() switch
                {
                    "weekly" => AggregateToWeekly(dailyAggs),
                    "monthly" => AggregateToMonthly(dailyAggs),
                    _ => _mapper.MapToTrendDataPoints(dailyAggs)
                };

                // Calculate summary
                var summary = new FloodTrendSummaryDto
                {
                    TotalFloodHours = dailyAggs.Sum(d => d.FloodHours),
                    AvgWaterLevel = (double)dailyAggs.Average(d => d.AvgLevel),
                    MaxWaterLevel = (double)dailyAggs.Max(d => d.MaxLevel),
                    MinWaterLevel = (double)dailyAggs.Min(d => d.MinLevel),
                    DaysWithFlooding = dailyAggs.Count(d => d.FloodHours > 0),
                    MostAffectedDay = dailyAggs.OrderByDescending(d => d.FloodHours)
                        .FirstOrDefault()?.Date.ToString("yyyy-MM-dd")
                };

                // Calculate comparison if requested
                FloodTrendComparisonDto? comparison = null;
                if (request.CompareWithPrevious)
                {
                    comparison = await CalculateComparison(
                        request.StationId, startDate, endDate, dailyAggs, ct);
                }

                var trendDto = new FloodTrendDto
                {
                    StationId = station.Id,
                    StationName = station.Name,
                    StationCode = station.Code,
                    Period = request.Period,
                    Granularity = request.Granularity,
                    DataPoints = dataPoints,
                    Comparison = comparison,
                    Summary = summary
                };

                return new GetFloodTrendsResponse
                {
                    Success = true,
                    Message = "Flood trends retrieved successfully",
                    StatusCode = FloodHistoryStatusCode.Success,
                    Data = trendDto
                };
            }
            catch (Exception ex)
            {
                return new GetFloodTrendsResponse
                {
                    Success = false,
                    Message = $"Error retrieving flood trends: {ex.Message}",
                    StatusCode = FloodHistoryStatusCode.InternalServerError
                };
            }
        }

        private (DateTime startDate, DateTime endDate) CalculateDateRange(
            string period, DateTime? customStart, DateTime? customEnd)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = period.ToLower() switch
            {
                "last7days" => endDate.AddDays(-7),
                "last30days" => endDate.AddDays(-30),
                "last90days" => endDate.AddDays(-90),
                "last365days" => endDate.AddDays(-365),
                "custom" => customStart?.Date ?? endDate.AddDays(-30),
                _ => endDate.AddDays(-30)
            };

            if (period.ToLower() == "custom" && customEnd.HasValue)
            {
                endDate = customEnd.Value.Date;
            }

            return (startDate, endDate);
        }

        private List<FloodTrendDataPointDto> AggregateToWeekly(
            List<Domain.RelationalDb.Entities.SensorDailyAgg> dailyAggs)
        {
            return dailyAggs
                .GroupBy(d => new { Year = d.Date.Year, Week = GetIso8601WeekOfYear(d.Date) })
                .Select(g => new FloodTrendDataPointDto
                {
                    Period = $"{g.Key.Year}-W{g.Key.Week:D2}",
                    PeriodStart = g.Min(d => d.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)),
                    PeriodEnd = g.Max(d => d.Date.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc)),
                    MaxLevel = (double)g.Max(d => d.MaxLevel),
                    MinLevel = (double)g.Min(d => d.MinLevel),
                    AvgLevel = (double)g.Average(d => d.AvgLevel),
                    ReadingCount = g.Sum(d => d.ReadingCount),
                    FloodHours = g.Sum(d => d.FloodHours),
                    RainfallTotal = g.Sum(d => d.RainfallTotal.HasValue ? (double)d.RainfallTotal.Value : 0),
                    PeakSeverity = GetPeakSeverity((double)g.Max(d => d.MaxLevel))
                })
                .OrderBy(d => d.PeriodStart)
                .ToList();
        }

        private List<FloodTrendDataPointDto> AggregateToMonthly(
            List<Domain.RelationalDb.Entities.SensorDailyAgg> dailyAggs)
        {
            return dailyAggs
                .GroupBy(d => new { d.Date.Year, d.Date.Month })
                .Select(g => new FloodTrendDataPointDto
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    PeriodStart = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    PeriodEnd = new DateTime(g.Key.Year, g.Key.Month,
                        DateTime.DaysInMonth(g.Key.Year, g.Key.Month), 23, 59, 59, DateTimeKind.Utc),
                    MaxLevel = (double)g.Max(d => d.MaxLevel),
                    MinLevel = (double)g.Min(d => d.MinLevel),
                    AvgLevel = (double)g.Average(d => d.AvgLevel),
                    ReadingCount = g.Sum(d => d.ReadingCount),
                    FloodHours = g.Sum(d => d.FloodHours),
                    RainfallTotal = g.Sum(d => d.RainfallTotal.HasValue ? (double)d.RainfallTotal.Value : 0),
                    PeakSeverity = GetPeakSeverity((double)g.Max(d => d.MaxLevel))
                })
                .OrderBy(d => d.PeriodStart)
                .ToList();
        }

        private async Task<FloodTrendComparisonDto> CalculateComparison(
            Guid stationId, DateTime startDate, DateTime endDate,
            List<Domain.RelationalDb.Entities.SensorDailyAgg> currentData,
            CancellationToken ct)
        {
            var periodDays = (endDate - startDate).Days;
            var prevStartDate = startDate.AddDays(-periodDays);
            var prevEndDate = startDate.AddDays(-1);

            var previousData = await _dailyAggRepository.GetByStationAndRangeAsync(
                stationId,
                DateOnly.FromDateTime(prevStartDate),
                DateOnly.FromDateTime(prevEndDate),
                ct);

            var comparison = new FloodTrendComparisonDto
            {
                PreviousPeriodStart = prevStartDate,
                PreviousPeriodEnd = prevEndDate
            };

            if (previousData.Any() && currentData.Any())
            {
                var currentAvg = (double)currentData.Average(d => d.AvgLevel);
                var previousAvg = (double)previousData.Average(d => d.AvgLevel);
                comparison.AvgLevelChange = previousAvg != 0
                    ? ((currentAvg - previousAvg) / previousAvg) * 100
                    : null;

                var currentFloodHours = currentData.Sum(d => d.FloodHours);
                var previousFloodHours = previousData.Sum(d => d.FloodHours);
                comparison.FloodHoursChange = previousFloodHours != 0
                    ? ((double)(currentFloodHours - previousFloodHours) / previousFloodHours) * 100
                    : null;

                var currentPeak = (double)currentData.Max(d => d.MaxLevel);
                var previousPeak = (double)previousData.Max(d => d.MaxLevel);
                comparison.PeakLevelChange = previousPeak != 0
                    ? ((currentPeak - previousPeak) / previousPeak) * 100
                    : null;
            }

            return comparison;
        }

        private static int GetIso8601WeekOfYear(DateOnly date)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);
            var day = System.Globalization.CultureInfo.InvariantCulture.Calendar
                .GetDayOfWeek(dateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                dateTime = dateTime.AddDays(3);
            }
            return System.Globalization.CultureInfo.InvariantCulture.Calendar
                .GetWeekOfYear(dateTime, System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Monday);
        }

        private static string GetPeakSeverity(double maxLevel)
        {
            if (maxLevel >= 300) return "critical";
            if (maxLevel >= 200) return "warning";
            if (maxLevel >= 100) return "caution";
            return "safe";
        }
    }
}
