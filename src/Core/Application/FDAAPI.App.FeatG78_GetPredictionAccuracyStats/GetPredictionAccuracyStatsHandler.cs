using FDAAPI.App.Common.DTOs;
using FDAAPI.App.FeatG76_LogPrediction;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG78_GetPredictionAccuracyStats
{
    public class GetPredictionAccuracyStatsHandler : IRequestHandler<GetPredictionAccuracyStatsRequest, GetPredictionAccuracyStatsResponse>
    {
        private readonly IPredictionLogRepository _predictionLogRepository;

        public GetPredictionAccuracyStatsHandler(IPredictionLogRepository predictionLogRepository)
        {
            _predictionLogRepository = predictionLogRepository;
        }

        public async Task<GetPredictionAccuracyStatsResponse> Handle(
            GetPredictionAccuracyStatsRequest request,
            CancellationToken ct)
        {
            // Get overall statistics
            var (totalPredictions, verifiedCount, correctCount, avgAccuracyScore) = 
                await _predictionLogRepository.GetAccuracyStatsAsync(
                    request.AreaId,
                    request.StartDate,
                    request.EndDate,
                    ct);

            var accuracyRate = verifiedCount > 0 
                ? (decimal)correctCount / verifiedCount 
                : 0;

            // For now, return overall stats only
            // TODO: Implement period grouping and area grouping if needed
            var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            return new GetPredictionAccuracyStatsResponse
            {
                Success = true,
                Message = "Prediction accuracy statistics retrieved successfully",
                StatusCode = PredictionLogStatusCode.Success,
                Data = new PredictionAccuracyStatsDto
                {
                    Period = new PredictionPeriodDto
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    Overall = new OverallPredictionStatsDto
                    {
                        TotalPredictions = totalPredictions,
                        VerifiedCount = verifiedCount,
                        CorrectCount = correctCount,
                        AccuracyRate = accuracyRate,
                        AvgAccuracyScore = avgAccuracyScore
                    },
                    ByPeriod = new List<PeriodPredictionStatsDto>(), // TODO: Implement period grouping
                    ByArea = new List<AreaPredictionStatsDto>() // TODO: Implement area grouping
                }
            };
        }
    }
}

