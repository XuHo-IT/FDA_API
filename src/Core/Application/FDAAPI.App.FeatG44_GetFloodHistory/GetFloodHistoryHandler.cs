using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.FloodHistory;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.App.FeatG44_GetFloodHistory;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG44_GetFloodHistory
{
    public class GetFloodHistoryHandler : IRequestHandler<GetFloodHistoryRequest, GetFloodHistoryResponse>
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;
        private readonly ISensorHourlyAggRepository _hourlyAggRepository;
        private readonly ISensorDailyAggRepository _dailyAggRepository;
        private readonly IStationRepository _stationRepository;
        private readonly IFloodHistoryMapper _mapper;

        public GetFloodHistoryHandler(
            ISensorReadingRepository sensorReadingRepository,
            ISensorHourlyAggRepository hourlyAggRepository,
            ISensorDailyAggRepository dailyAggRepository,
            IStationRepository stationRepository,
            IFloodHistoryMapper mapper)
        {
            _sensorReadingRepository = sensorReadingRepository;
            _hourlyAggRepository = hourlyAggRepository;
            _dailyAggRepository = dailyAggRepository;
            _stationRepository = stationRepository;
            _mapper = mapper;
        }

        public async Task<GetFloodHistoryResponse> Handle(
            GetFloodHistoryRequest request,
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
                    return new GetFloodHistoryResponse
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
                    return new GetFloodHistoryResponse
                    {
                        Success = false,
                        Message = "Station not found",
                        StatusCode = FloodHistoryStatusCode.NotFound
                    };
                }

                // Set default date range (last 24 hours)
                var endDate = request.EndDate ?? DateTime.UtcNow;
                var startDate = request.StartDate ?? endDate.AddHours(-24);

                // Get data based on granularity
                List<FloodDataPointDto> dataPoints;
                var granularity = request.Granularity.ToLower();

                switch (granularity)
                {
                    case "raw":
                        dataPoints = await GetRawDataPoints(stationId.Value, startDate, endDate, request.Limit, ct);
                        break;
                    case "hourly":
                        dataPoints = await GetHourlyDataPoints(stationId.Value, startDate, endDate, ct);
                        break;
                    case "daily":
                        dataPoints = await GetDailyDataPoints(stationId.Value, startDate, endDate, ct);
                        break;
                    default:
                        dataPoints = await GetHourlyDataPoints(stationId.Value, startDate, endDate, ct);
                        break;
                }

                // Detect missing intervals
                var missingCount = DetectMissingIntervals(dataPoints, granularity);

                // Build response
                var historyDto = new FloodHistoryDto
                {
                    StationId = station.Id,
                    StationName = station.Name,
                    StationCode = station.Code,
                    DataPoints = dataPoints.Take(request.Limit).ToList(),
                    Metadata = new FloodHistoryMetadataDto
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        Granularity = granularity,
                        TotalDataPoints = dataPoints.Count,
                        MissingIntervals = missingCount,
                        LastUpdated = dataPoints.Any() ? dataPoints.Max(d => d.Timestamp) : null
                    }
                };

                return new GetFloodHistoryResponse
                {
                    Success = true,
                    Message = "Flood history retrieved successfully",
                    StatusCode = FloodHistoryStatusCode.Success,
                    Data = historyDto,
                    Pagination = new PaginationDto
                    {
                        HasMore = dataPoints.Count > request.Limit,
                        TotalCount = dataPoints.Count
                    }
                };
            }
            catch (Exception ex)
            {
                return new GetFloodHistoryResponse
                {
                    Success = false,
                    Message = $"Error retrieving flood history: {ex.Message}",
                    StatusCode = FloodHistoryStatusCode.InternalServerError
                };
            }
        }

        private async Task<List<FloodDataPointDto>> GetRawDataPoints(
            Guid stationId, DateTime startDate, DateTime endDate, int limit, CancellationToken ct)
        {
            var readings = await _sensorReadingRepository.GetByStationAndTimeRangeAsync(
                stationId, startDate, endDate, limit, ct);

            return _mapper.MapToDataPoints(readings);
        }

        private async Task<List<FloodDataPointDto>> GetHourlyDataPoints(
            Guid stationId, DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            var hourlyAggs = await _hourlyAggRepository.GetByStationAndRangeAsync(
                stationId, startDate, endDate, ct);

            return _mapper.MapToDataPoints(hourlyAggs);
        }

        private async Task<List<FloodDataPointDto>> GetDailyDataPoints(
            Guid stationId, DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            var dailyAggs = await _dailyAggRepository.GetByStationAndRangeAsync(
                stationId,
                DateOnly.FromDateTime(startDate),
                DateOnly.FromDateTime(endDate),
                ct);

            return _mapper.MapToDataPoints(dailyAggs);
        }

        private int DetectMissingIntervals(List<FloodDataPointDto> dataPoints, string granularity)
        {
            if (dataPoints.Count < 2) return 0;

            var expectedInterval = granularity switch
            {
                "raw" => TimeSpan.FromMinutes(5),
                "hourly" => TimeSpan.FromHours(1),
                "daily" => TimeSpan.FromDays(1),
                _ => TimeSpan.FromHours(1)
            };

            var missingCount = 0;
            for (int i = 1; i < dataPoints.Count; i++)
            {
                var gap = dataPoints[i].Timestamp - dataPoints[i - 1].Timestamp;
                if (gap > expectedInterval * 1.5)
                {
                    missingCount++;
                }
            }

            return missingCount;
        }
    }
}
