using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.FeatG79_VerifyPredictions
{
    public class VerifyPredictionsRunner
    {
        private readonly IPredictionLogRepository _predictionLogRepository;
        private readonly IAreaRepository _areaRepository;
        private readonly IAdministrativeAreaRepository _administrativeAreaRepository;
        private readonly IStationRepository _stationRepository;
        private readonly ISensorReadingRepository _sensorReadingRepository;
        private readonly ILogger<VerifyPredictionsRunner> _logger;

        public VerifyPredictionsRunner(
            IPredictionLogRepository predictionLogRepository,
            IAreaRepository areaRepository,
            IAdministrativeAreaRepository administrativeAreaRepository,
            IStationRepository stationRepository,
            ISensorReadingRepository sensorReadingRepository,
            ILogger<VerifyPredictionsRunner> logger)
        {
            _predictionLogRepository = predictionLogRepository;
            _areaRepository = areaRepository;
            _administrativeAreaRepository = administrativeAreaRepository;
            _stationRepository = stationRepository;
            _sensorReadingRepository = sensorReadingRepository;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Hangfire triggered prediction verification job");

            try
            {
                // Get pending predictions (EndTime <= Now and IsVerified = false)
                var pendingPredictions = await _predictionLogRepository.GetPendingVerificationAsync(
                    DateTime.UtcNow,
                    limit: 100,
                    ct: default);

                if (!pendingPredictions.Any())
                {
                    _logger.LogInformation("No pending predictions to verify");
                    return;
                }

                _logger.LogInformation($"Found {pendingPredictions.Count} pending predictions to verify");

                int verifiedCount = 0;
                int errorCount = 0;

                foreach (var prediction in pendingPredictions)
                {
                    try
                    {
                        await VerifyPredictionAsync(prediction);
                        verifiedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error verifying prediction {prediction.Id}");
                        errorCount++;
                    }
                }

                _logger.LogInformation($"Prediction verification completed: {verifiedCount} verified, {errorCount} errors");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in prediction verification job");
                throw;
            }
        }

        private async Task VerifyPredictionAsync(PredictionLog prediction)
        {
            List<Station> stations = new List<Station>();

            // 1. Determine if this is for Area or AdministrativeArea
            if (prediction.AreaId.HasValue)
            {
                // For user-created Area
                var area = await _areaRepository.GetByIdAsync(prediction.AreaId.Value, default);
                if (area == null)
                {
                    _logger.LogWarning($"Area {prediction.AreaId} not found for prediction {prediction.Id}");
                    return;
                }

                // Get stations within area radius
                stations = (await _stationRepository.GetStationsWithinRadiusAsync(
                    area.Latitude,
                    area.Longitude,
                    area.RadiusMeters,
                    default)).ToList();
            }
            else if (prediction.AdministrativeAreaId.HasValue)
            {
                // For AdministrativeArea - get stations using same logic as AdministrativeAreasEvaluateHandler
                var administrativeArea = await _administrativeAreaRepository.GetByIdAsync(prediction.AdministrativeAreaId.Value, default);
                if (administrativeArea == null)
                {
                    _logger.LogWarning($"AdministrativeArea {prediction.AdministrativeAreaId} not found for prediction {prediction.Id}");
                    return;
                }

                var level = administrativeArea.Level.ToLower();
                
                if (level == "ward")
                {
                    var (allStations, _) = await _stationRepository.GetStationsAsync(
                        searchTerm: null,
                        status: "active",
                        pageNumber: 1,
                        pageSize: 1000,
                        default);
                    stations = allStations
                        .Where(s => s.AdministrativeAreaId == administrativeArea.Id)
                        .ToList();
                }
                else if (level == "district")
                {
                    var (childWards, _) = await _administrativeAreaRepository.GetAdministrativeAreasAsync(
                        searchTerm: null,
                        level: "ward",
                        parentId: administrativeArea.Id,
                        pageNumber: 1,
                        pageSize: 1000,
                        default);
                    var wardIds = childWards.Select(w => w.Id).ToList();

                    if (wardIds.Any())
                    {
                        var (allStations, _) = await _stationRepository.GetStationsAsync(
                            searchTerm: null,
                            status: "active",
                            pageNumber: 1,
                            pageSize: 10000,
                            default);
                        stations = allStations
                            .Where(s => s.AdministrativeAreaId.HasValue && wardIds.Contains(s.AdministrativeAreaId.Value))
                            .ToList();
                    }
                }
                else if (level == "city")
                {
                    var (districts, _) = await _administrativeAreaRepository.GetAdministrativeAreasAsync(
                        searchTerm: null,
                        level: "district",
                        parentId: administrativeArea.Id,
                        pageNumber: 1,
                        pageSize: 1000,
                        default);

                    var allWardIds = new List<Guid>();
                    foreach (var district in districts)
                    {
                        var (wards, _) = await _administrativeAreaRepository.GetAdministrativeAreasAsync(
                            searchTerm: null,
                            level: "ward",
                            parentId: district.Id,
                            pageNumber: 1,
                            pageSize: 1000,
                            default);
                        allWardIds.AddRange(wards.Select(w => w.Id));
                    }

                    if (allWardIds.Any())
                    {
                        var (allStations, _) = await _stationRepository.GetStationsAsync(
                            searchTerm: null,
                            status: "active",
                            pageNumber: 1,
                            pageSize: 10000,
                            default);
                        stations = allStations
                            .Where(s => s.AdministrativeAreaId.HasValue && allWardIds.Contains(s.AdministrativeAreaId.Value))
                            .ToList();
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Prediction {prediction.Id} has neither AreaId nor AdministrativeAreaId");
                return;
            }

            if (!stations.Any())
            {
                var areaInfo = prediction.AreaId.HasValue 
                    ? $"Area {prediction.AreaId}" 
                    : $"AdministrativeArea {prediction.AdministrativeAreaId}";
                _logger.LogWarning($"No stations found for {areaInfo} for prediction {prediction.Id}");
                // Mark as verified but with no data
                prediction.ActualWaterLevel = null;
                prediction.IsVerified = true;
                prediction.IsCorrect = null;
                prediction.AccuracyScore = null;
                prediction.VerifiedAt = DateTime.UtcNow;
                await _predictionLogRepository.UpdateAsync(prediction, default);
                return;
            }

            // 3. Get sensor readings for all stations in time range
            var stationIds = stations.Select(s => s.Id).ToList();
            var allReadings = new List<SensorReading>();

            foreach (var stationId in stationIds)
            {
                var readings = await _sensorReadingRepository.GetByStationAndTimeRangeAsync(
                    stationId,
                    prediction.StartTime,
                    prediction.EndTime,
                    limit: 10000,
                    default);
                allReadings.AddRange(readings);
            }

            // 4. Calculate actual water level (MAX value)
            var actualWaterLevel = allReadings.Any()
                ? (decimal?)allReadings.Max(r => r.Value)
                : null;

            // 5. Determine if prediction is correct
            bool? isCorrect = null;
            decimal? accuracyScore = null;

            if (actualWaterLevel.HasValue)
            {
                // Convert actual water level from cm to meters (assuming Value is in cm)
                var actualWaterLevelMeters = actualWaterLevel.Value / 100m;

                // Logic: If actual < 0.2m (safe) and predicted < 0.3 (low risk) → Correct
                //        If actual >= 0.2m (flood) and predicted >= 0.3 (risk) → Correct
                //        Otherwise → Incorrect
                var isFlood = actualWaterLevelMeters >= 0.2m;
                var isPredictedRisk = prediction.PredictedProb >= 0.3m;

                isCorrect = (isFlood && isPredictedRisk) || (!isFlood && !isPredictedRisk);

                // Calculate accuracy score
                if (isCorrect == true)
                {
                    accuracyScore = 1.0m;
                }
                else
                {
                    // Calculate difference between predicted and actual normalized probability
                    var actualNormalizedProb = Math.Min(1.0m, actualWaterLevelMeters / 1.0m); // Normalize to 0-1
                    var diff = Math.Abs(prediction.PredictedProb - actualNormalizedProb);
                    accuracyScore = Math.Max(0m, 1.0m - diff);
                }
            }

            // 6. Update prediction log
            prediction.ActualWaterLevel = actualWaterLevel;
            prediction.IsVerified = true;
            prediction.IsCorrect = isCorrect;
            prediction.AccuracyScore = accuracyScore;
            prediction.VerifiedAt = DateTime.UtcNow;
            prediction.UpdatedAt = DateTime.UtcNow;

            await _predictionLogRepository.UpdateAsync(prediction, default);

            _logger.LogInformation(
                $"Verified prediction {prediction.Id}: Actual={actualWaterLevel}, IsCorrect={isCorrect}, Accuracy={accuracyScore}");
        }
    }
}

