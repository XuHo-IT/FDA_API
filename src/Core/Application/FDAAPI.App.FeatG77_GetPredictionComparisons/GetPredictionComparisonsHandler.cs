using FDAAPI.App.Common.DTOs;
using FDAAPI.App.FeatG76_LogPrediction;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG77_GetPredictionComparisons
{
    public class GetPredictionComparisonsHandler : IRequestHandler<GetPredictionComparisonsRequest, GetPredictionComparisonsResponse>
    {
        private readonly IPredictionLogRepository _predictionLogRepository;

        public GetPredictionComparisonsHandler(IPredictionLogRepository predictionLogRepository)
        {
            _predictionLogRepository = predictionLogRepository;
        }

        public async Task<GetPredictionComparisonsResponse> Handle(
            GetPredictionComparisonsRequest request,
            CancellationToken ct)
        {
            // Default to verified predictions only
            var isVerified = request.IsVerified ?? true;

            // Get verified predictions with filters
            var (items, totalCount) = await _predictionLogRepository.GetVerifiedAsync(
                request.AreaId,
                request.StartDate,
                request.EndDate,
                request.MinAccuracy,
                request.Page,
                request.Size,
                ct);

            // Get summary statistics
            var (totalPredictions, verifiedCount, correctCount, avgAccuracyScore) = 
                await _predictionLogRepository.GetAccuracyStatsAsync(
                    request.AreaId,
                    request.StartDate,
                    request.EndDate,
                    ct);

            // Map to DTOs
            var comparisonDtos = items.Select(p => new PredictionLogDto
            {
                PredictionLogId = p.Id,
                AreaId = p.AreaId,
                AreaName = p.Area?.Name,
                PredictedProb = p.PredictedProb,
                AiProb = p.AiProb,
                PhysicsProb = p.PhysicsProb,
                RiskLevel = p.RiskLevel,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                ActualWaterLevel = p.ActualWaterLevel,
                IsVerified = p.IsVerified,
                IsCorrect = p.IsCorrect,
                AccuracyScore = p.AccuracyScore,
                VerifiedAt = p.VerifiedAt,
                CreatedAt = p.CreatedAt
            }).ToList();

            var accuracyRate = verifiedCount > 0 
                ? (decimal)correctCount / verifiedCount 
                : 0;

            return new GetPredictionComparisonsResponse
            {
                Success = true,
                Message = "Prediction comparisons retrieved successfully",
                StatusCode = PredictionLogStatusCode.Success,
                Data = new PredictionComparisonsDataDto
                {
                    Total = totalCount,
                    Items = comparisonDtos,
                    Summary = new PredictionComparisonSummaryDto
                    {
                        TotalPredictions = totalPredictions,
                        VerifiedCount = verifiedCount,
                        CorrectCount = correctCount,
                        AccuracyRate = accuracyRate,
                        AvgAccuracyScore = avgAccuracyScore
                    }
                }
            };
        }
    }
}

