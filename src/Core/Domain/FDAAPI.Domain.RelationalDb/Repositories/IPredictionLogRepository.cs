using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IPredictionLogRepository
    {
        Task<Guid> CreateAsync(PredictionLog entity, CancellationToken ct = default);
        Task<PredictionLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<PredictionLog>> GetPendingVerificationAsync(DateTime beforeTime, int limit, CancellationToken ct = default);
        Task<(List<PredictionLog> Items, int TotalCount)> GetVerifiedAsync(
            Guid? areaId, 
            DateTime? startDate, 
            DateTime? endDate, 
            decimal? minAccuracy,
            int page, 
            int size, 
            CancellationToken ct = default);
        Task<bool> UpdateAsync(PredictionLog entity, CancellationToken ct = default);
        Task<(int TotalPredictions, int VerifiedCount, int CorrectCount, decimal AvgAccuracyScore)> GetAccuracyStatsAsync(
            Guid? areaId,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken ct = default);
    }
}

