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

namespace FDAAPI.App.FeatG46_GetFloodStatistics
{
    public class GetFloodStatisticsHandler : IRequestHandler<GetFloodStatisticsRequest, GetFloodStatisticsResponse>
    {
        private readonly ISensorDailyAggRepository _dailyAggRepository;
        private readonly IStationRepository _stationRepository;
        private readonly IFloodHistoryMapper _mapper;

        public GetFloodStatisticsHandler(
            ISensorDailyAggRepository dailyAggRepository,
            IStationRepository stationRepository,
            IFloodHistoryMapper mapper)
        {
            _dailyAggRepository = dailyAggRepository;
            _stationRepository = stationRepository;
            _mapper = mapper;
        }

        public async Task<GetFloodStatisticsResponse> Handle(
            GetFloodStatisticsRequest request,
            CancellationToken ct)
        {
            try
            {
                // Determine station ID
                var stationId = request.StationId;
                if (!stationId.HasValue && request.StationIds?.Any() == true)
                {
                    stationId = request.StationIds.First();
                }

                if (!stationId.HasValue)
                {
                    return new GetFloodStatisticsResponse
                    {
                        Success = false,
                        Message = "StationId is required",
                        StatusCode = FloodHistoryStatusCode.BadRequest
                    };
                }

                // Get station info
                var station = await _stationRepository.GetByIdAsync(stationId.Value, ct);
                if (station == null)
                {
                    return new GetFloodStatisticsResponse
                    {
                        Success = false,
                        Message = "Station not found",
                        StatusCode = FloodHistoryStatusCode.NotFound
                    };
                }

                // Calculate date range
                var (startDate, endDate) = CalculateDateRange(request.Period);

                // Get daily aggregates
                var dailyAggs = await _dailyAggRepository.GetByStationAndRangeAsync(
                    stationId.Value,
                    DateOnly.FromDateTime(startDate),
                    DateOnly.FromDateTime(endDate),
                    ct);

                // Calculate statistics
                var summary = CalculateSummary(dailyAggs, startDate, endDate);

                // Calculate severity breakdown if requested
                FloodStatisticsSeverityBreakdownDto? severityBreakdown = null;
                if (request.IncludeBreakdown)
                {
                    severityBreakdown = CalculateSeverityBreakdown(dailyAggs);
                }

                // Calculate comparison if requested
                FloodTrendComparisonDto? comparison = null;
                if (request.IncludeComparison)
                {
                    comparison = await CalculateComparison(stationId.Value, startDate, endDate, dailyAggs, ct);
                }

                // Calculate data quality
                var dataQuality = CalculateDataQuality(dailyAggs, startDate, endDate);

                var statisticsDto = new FloodStatisticsDto
                {
                    StationId = station.Id,
                    StationName = station.Name,
                    StationCode = station.Code,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    Summary = summary,
                    SeverityBreakdown = severityBreakdown,
                    Comparison = comparison,
                    DataQuality = dataQuality
                };

                return new GetFloodStatisticsResponse
                {
                    Success = true,
                    Message = "Flood statistics retrieved successfully",
                    StatusCode = FloodHistoryStatusCode.Success,
                    Data = statisticsDto
                };
            }
            catch (Exception ex)
            {
                return new GetFloodStatisticsResponse
                {
                    Success = false,
                    Message = $"Error retrieving flood statistics: {ex.Message}",
                    StatusCode = FloodHistoryStatusCode.InternalServerError
                };
            }
        }

        private (DateTime startDate, DateTime endDate) CalculateDateRange(string period)
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = period.ToLower() switch
            {
                "last7days" => endDate.AddDays(-7),
                "last30days" => endDate.AddDays(-30),
                "last90days" => endDate.AddDays(-90),
                "last365days" => endDate.AddDays(-365),
                _ => endDate.AddDays(-30)
            };
            return (startDate, endDate);
        }

        private FloodStatisticsSummaryDto CalculateSummary(
            List<Domain.RelationalDb.Entities.SensorDailyAgg> dailyAggs,
            DateTime startDate, DateTime endDate)
        {
            if (!dailyAggs.Any())
            {
                return new FloodStatisticsSummaryDto();
            }

            var expectedDays = (endDate - startDate).Days + 1;
            var actualDays = dailyAggs.Count;

            return new FloodStatisticsSummaryDto
            {
                MaxWaterLevel = (double)dailyAggs.Max(d => d.MaxLevel),
                MinWaterLevel = (double)dailyAggs.Min(d => d.MinLevel),
                AvgWaterLevel = (double)dailyAggs.Average(d => d.AvgLevel),
                TotalFloodHours = dailyAggs.Sum(d => d.FloodHours),
                TotalReadings = dailyAggs.Sum(d => d.ReadingCount),
                MissingIntervals = expectedDays - actualDays
            };
        }

        private FloodStatisticsSeverityBreakdownDto CalculateSeverityBreakdown(
            List<Domain.RelationalDb.Entities.SensorDailyAgg> dailyAggs)
        {
            // Estimate hours per severity based on peak severity per day
            // This is a simplified calculation
            var totalHours = dailyAggs.Count * 24;
            var floodHours = dailyAggs.Sum(d => d.FloodHours);
            var safeHours = totalHours - floodHours;

            // Distribute flood hours by peak severity
            var criticalDays = dailyAggs.Count(d => d.PeakSeverity == 3);
            var warningDays = dailyAggs.Count(d => d.PeakSeverity == 2);
            var cautionDays = dailyAggs.Count(d => d.PeakSeverity == 1);

            return new FloodStatisticsSeverityBreakdownDto
            {
                HoursSafe = safeHours,
                HoursCaution = dailyAggs.Where(d => d.PeakSeverity == 1).Sum(d => d.FloodHours),
                HoursWarning = dailyAggs.Where(d => d.PeakSeverity == 2).Sum(d => d.FloodHours),
                HoursCritical = dailyAggs.Where(d => d.PeakSeverity == 3).Sum(d => d.FloodHours)
            };
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
                    ? Math.Round(((currentAvg - previousAvg) / previousAvg) * 100, 2)
                    : null;

                var currentFloodHours = currentData.Sum(d => d.FloodHours);
                var previousFloodHours = previousData.Sum(d => d.FloodHours);
                comparison.FloodHoursChange = previousFloodHours != 0
                    ? Math.Round(((double)(currentFloodHours - previousFloodHours) / previousFloodHours) * 100, 2)
                    : null;
            }

            return comparison;
        }

        private FloodDataQualityDto CalculateDataQuality(
            List<Domain.RelationalDb.Entities.SensorDailyAgg> dailyAggs,
            DateTime startDate, DateTime endDate)
        {
            var expectedDays = (endDate - startDate).Days + 1;
            var actualDays = dailyAggs.Count;
            var completeness = expectedDays > 0 ? (actualDays * 100.0 / expectedDays) : 0;

            // Detect missing intervals
            var missingIntervals = new List<MissingIntervalDto>();
            var orderedData = dailyAggs.OrderBy(d => d.Date).ToList();

            for (int i = 1; i < orderedData.Count; i++)
            {
                var gap = orderedData[i].Date.DayNumber - orderedData[i - 1].Date.DayNumber;
                if (gap > 1)
                {
                    missingIntervals.Add(new MissingIntervalDto
                    {
                        Start = orderedData[i - 1].Date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc),
                        End = orderedData[i].Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                        DurationMinutes = (gap - 1) * 24 * 60
                    });
                }
            }

            return new FloodDataQualityDto
            {
                Completeness = Math.Round(completeness, 2),
                MissingIntervals = missingIntervals
            };
        }
    }
}
