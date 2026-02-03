using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG76_LogPrediction
{
    public class LogPredictionHandler : IRequestHandler<LogPredictionRequest, LogPredictionResponse>
    {
        private readonly IPredictionLogRepository _predictionLogRepository;
        private readonly IAreaRepository _areaRepository;
        private readonly IAdministrativeAreaRepository _administrativeAreaRepository;
        private readonly IStationRepository _stationRepository;

        public LogPredictionHandler(
            IPredictionLogRepository predictionLogRepository,
            IAreaRepository areaRepository,
            IAdministrativeAreaRepository administrativeAreaRepository,
            IStationRepository stationRepository)
        {
            _predictionLogRepository = predictionLogRepository;
            _areaRepository = areaRepository;
            _administrativeAreaRepository = administrativeAreaRepository;
            _stationRepository = stationRepository;
        }

        public async Task<LogPredictionResponse> Handle(LogPredictionRequest request, CancellationToken ct)
        {
            // 1. Verify AdministrativeArea exists
            var administrativeArea = await _administrativeAreaRepository.GetByIdAsync(request.AdministrativeAreaId, ct);
            if (administrativeArea == null)
            {
                return new LogPredictionResponse
                {
                    Success = false,
                    Message = $"Administrative Area with ID {request.AdministrativeAreaId} not found.",
                    StatusCode = PredictionLogStatusCode.NotFound
                };
            }

            // 2. Get all stations in the AdministrativeArea (similar to AdministrativeAreasEvaluateHandler)
            var level = administrativeArea.Level.ToLower();
            List<Station> stationsInArea = new List<Station>();

            if (level == "ward")
            {
                // Get stations directly in the ward
                var (allStations, _) = await _stationRepository.GetStationsAsync(
                    searchTerm: null,
                    status: "active",
                    pageNumber: 1,
                    pageSize: 1000,
                    ct);
                stationsInArea = allStations
                    .Where(s => s.AdministrativeAreaId == administrativeArea.Id)
                    .ToList();
            }
            else if (level == "district")
            {
                // Get all child wards
                var (childWards, _) = await _administrativeAreaRepository.GetAdministrativeAreasAsync(
                    searchTerm: null,
                    level: "ward",
                    parentId: administrativeArea.Id,
                    pageNumber: 1,
                    pageSize: 1000,
                    ct);
                var wardIds = childWards.Select(w => w.Id).ToList();

                if (wardIds.Any())
                {
                    var (allStations, _) = await _stationRepository.GetStationsAsync(
                        searchTerm: null,
                        status: "active",
                        pageNumber: 1,
                        pageSize: 10000,
                        ct);
                    stationsInArea = allStations
                        .Where(s => s.AdministrativeAreaId.HasValue && wardIds.Contains(s.AdministrativeAreaId.Value))
                        .ToList();
                }
            }
            else if (level == "city")
            {
                // Get all districts, then all wards
                var (districts, _) = await _administrativeAreaRepository.GetAdministrativeAreasAsync(
                    searchTerm: null,
                    level: "district",
                    parentId: administrativeArea.Id,
                    pageNumber: 1,
                    pageSize: 1000,
                    ct);

                var allWardIds = new List<Guid>();
                foreach (var district in districts)
                {
                    var (wards, _) = await _administrativeAreaRepository.GetAdministrativeAreasAsync(
                        searchTerm: null,
                        level: "ward",
                        parentId: district.Id,
                        pageNumber: 1,
                        pageSize: 1000,
                        ct);
                    allWardIds.AddRange(wards.Select(w => w.Id));
                }

                if (allWardIds.Any())
                {
                    var (allStations, _) = await _stationRepository.GetStationsAsync(
                        searchTerm: null,
                        status: "active",
                        pageNumber: 1,
                        pageSize: 10000,
                        ct);
                    stationsInArea = allStations
                        .Where(s => s.AdministrativeAreaId.HasValue && allWardIds.Contains(s.AdministrativeAreaId.Value))
                        .ToList();
                }
            }

            if (!stationsInArea.Any())
            {
                return new LogPredictionResponse
                {
                    Success = false,
                    Message = $"No active stations found in Administrative Area {administrativeArea.Name} (Level: {level}).",
                    StatusCode = PredictionLogStatusCode.NotFound
                };
            }

            // 3. Create prediction log directly for AdministrativeArea
            // Simple approach: AI sends AdministrativeAreaId → BE saves it directly
            // No need to find user-created Areas - this is for AdministrativeArea prediction
            var now = DateTime.UtcNow;
            
            var predictionLog = new PredictionLog
            {
                Id = Guid.NewGuid(),
                AreaId = null,  // Not for user-created Area
                AdministrativeAreaId = administrativeArea.Id,  // Directly save AdministrativeAreaId
                PredictedProb = request.PredictedProb,
                AiProb = request.AiProb,
                PhysicsProb = request.PhysicsProb,
                RiskLevel = request.RiskLevel.ToLower(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsVerified = false,
                CreatedBy = Guid.Empty, // Internal API - no user context
                CreatedAt = now,
                UpdatedBy = Guid.Empty,
                UpdatedAt = now
            };

            var predictionLogId = await _predictionLogRepository.CreateAsync(predictionLog, ct);

            // 4. Return response
            return new LogPredictionResponse
            {
                Success = true,
                Message = $"Prediction logged successfully for Administrative Area {administrativeArea.Name} ({level}).",
                StatusCode = PredictionLogStatusCode.Created,
                Data = new LogPredictionDataDto
                {
                    PredictionLogId = predictionLogId,
                    AreaId = null,  // Not applicable for AdministrativeArea predictions
                    AdministrativeAreaId = administrativeArea.Id,  // For AdministrativeArea predictions
                    IsVerified = false,
                    CreatedAt = now
                }
            };
        }
    }
}

