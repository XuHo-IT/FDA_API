using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlPredictionLogRepository : IPredictionLogRepository
    {
        private readonly AppDbContext _context;

        public PgsqlPredictionLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(PredictionLog entity, CancellationToken ct = default)
        {
            _context.PredictionLogs.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<PredictionLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.PredictionLogs
                .Include(p => p.Area)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<List<PredictionLog>> GetPendingVerificationAsync(DateTime beforeTime, int limit, CancellationToken ct = default)
        {
            return await _context.PredictionLogs
                .Where(p => !p.IsVerified && p.EndTime <= beforeTime)
                .OrderBy(p => p.EndTime)
                .Take(limit)
                .ToListAsync(ct);
        }

        public async Task<(List<PredictionLog> Items, int TotalCount)> GetVerifiedAsync(
            Guid? areaId,
            DateTime? startDate,
            DateTime? endDate,
            decimal? minAccuracy,
            int page,
            int size,
            CancellationToken ct = default)
        {
            var query = _context.PredictionLogs
                .Include(p => p.Area)
                .AsNoTracking()
                .Where(p => p.IsVerified);

            if (areaId.HasValue)
            {
                query = query.Where(p => p.AreaId == areaId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.EndTime <= endDate.Value);
            }

            if (minAccuracy.HasValue)
            {
                query = query.Where(p => p.AccuracyScore.HasValue && p.AccuracyScore >= minAccuracy.Value);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(p => p.StartTime)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<bool> UpdateAsync(PredictionLog entity, CancellationToken ct = default)
        {
            _context.PredictionLogs.Update(entity);
            var result = await _context.SaveChangesAsync(ct);
            return result > 0;
        }

        public async Task<(int TotalPredictions, int VerifiedCount, int CorrectCount, decimal AvgAccuracyScore)> GetAccuracyStatsAsync(
            Guid? areaId,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken ct = default)
        {
            var query = _context.PredictionLogs
                .AsNoTracking()
                .AsQueryable();

            if (areaId.HasValue)
            {
                query = query.Where(p => p.AreaId == areaId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.EndTime <= endDate.Value);
            }

            var totalPredictions = await query.CountAsync(ct);
            var verifiedCount = await query.CountAsync(p => p.IsVerified, ct);
            var correctCount = await query.CountAsync(p => p.IsVerified && p.IsCorrect == true, ct);
            
            // Fix: DefaultIfEmpty cannot be translated to SQL, so we need to handle it differently
            var verifiedWithScores = await query
                .Where(p => p.IsVerified && p.AccuracyScore.HasValue)
                .Select(p => p.AccuracyScore!.Value)
                .ToListAsync(ct);
            
            var avgAccuracyScore = verifiedWithScores.Any() 
                ? (decimal)verifiedWithScores.Average() 
                : 0m;

            return (totalPredictions, verifiedCount, correctCount, avgAccuracyScore);
        }
    }
}

