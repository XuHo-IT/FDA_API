using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG76_LogPrediction
{
    public class LogPredictionHandler : IRequestHandler<LogPredictionRequest, LogPredictionResponse>
    {
        private readonly IPredictionLogRepository _predictionLogRepository;
        private readonly IAreaRepository _areaRepository;

        public LogPredictionHandler(
            IPredictionLogRepository predictionLogRepository,
            IAreaRepository areaRepository)
        {
            _predictionLogRepository = predictionLogRepository;
            _areaRepository = areaRepository;
        }

        public async Task<LogPredictionResponse> Handle(LogPredictionRequest request, CancellationToken ct)
        {
            // Verify area exists
            var area = await _areaRepository.GetByIdAsync(request.AreaId, ct);
            if (area == null)
            {
                return new LogPredictionResponse
                {
                    Success = false,
                    Message = $"Area with ID {request.AreaId} not found.",
                    StatusCode = PredictionLogStatusCode.NotFound
                };
            }

            // Create prediction log entity
            var predictionLog = new PredictionLog
            {
                Id = Guid.NewGuid(),
                AreaId = request.AreaId,
                PredictedProb = request.PredictedProb,
                AiProb = request.AiProb,
                PhysicsProb = request.PhysicsProb,
                RiskLevel = request.RiskLevel.ToLower(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsVerified = false,
                CreatedBy = Guid.Empty, // Internal API - no user context
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.Empty,
                UpdatedAt = DateTime.UtcNow
            };

            var predictionLogId = await _predictionLogRepository.CreateAsync(predictionLog, ct);

            return new LogPredictionResponse
            {
                Success = true,
                Message = "Prediction logged successfully",
                StatusCode = PredictionLogStatusCode.Created,
                Data = new LogPredictionDataDto
                {
                    PredictionLogId = predictionLogId,
                    AreaId = request.AreaId,
                    IsVerified = false,
                    CreatedAt = predictionLog.CreatedAt
                }
            };
        }
    }
}

