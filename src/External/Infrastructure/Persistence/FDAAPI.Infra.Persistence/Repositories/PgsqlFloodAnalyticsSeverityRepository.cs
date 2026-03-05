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
    public class PgsqlFloodAnalyticsSeverityRepository : IFloodAnalyticsSeverityRepository
    {
        private readonly AppDbContext _context;

        public PgsqlFloodAnalyticsSeverityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FloodAnalyticsSeverity>> GetByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            string bucketType,
            CancellationToken ct = default)
        {
            return await _context.FloodAnalyticsSeverities
                .AsNoTracking()
                .Where(s => s.AdministrativeAreaId == administrativeAreaId
                    && s.BucketType == bucketType
                    && s.TimeBucket >= startDate
                    && s.TimeBucket < endDate)
                .OrderBy(s => s.TimeBucket)
                .ToListAsync(ct);
        }

        public async Task<FloodAnalyticsSeverity?> GetByAdministrativeAreaBucketAsync(
            Guid administrativeAreaId,
            DateTime timeBucket,
            string bucketType,
            CancellationToken ct = default)
        {
            return await _context.FloodAnalyticsSeverities
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.AdministrativeAreaId == administrativeAreaId
                    && s.TimeBucket == timeBucket
                    && s.BucketType == bucketType, ct);
        }

        public async Task BulkUpsertAsync(
            List<FloodAnalyticsSeverity> aggregates,
            CancellationToken ct = default)
        {
            if (!aggregates.Any())
                return;

            // Use raw SQL for PostgreSQL ON CONFLICT DO UPDATE (idempotent upsert)
            foreach (var agg in aggregates)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO ""FloodAnalyticsSeverity"" 
                        (""Id"", ""AdministrativeAreaId"", ""TimeBucket"", ""BucketType"", ""MaxLevel"", ""AvgLevel"", ""MinLevel"", ""DurationHours"", ""ReadingCount"", ""CalculatedAt"")
                      VALUES 
                        ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})
                      ON CONFLICT (""AdministrativeAreaId"", ""TimeBucket"", ""BucketType"")
                      DO UPDATE SET
                        ""MaxLevel"" = EXCLUDED.""MaxLevel"",
                        ""AvgLevel"" = EXCLUDED.""AvgLevel"",
                        ""MinLevel"" = EXCLUDED.""MinLevel"",
                        ""DurationHours"" = EXCLUDED.""DurationHours"",
                        ""ReadingCount"" = EXCLUDED.""ReadingCount"",
                        ""CalculatedAt"" = EXCLUDED.""CalculatedAt""",
                    new object[]
                    {
                        agg.Id,
                        agg.AdministrativeAreaId,
                        agg.TimeBucket,
                        agg.BucketType,
                        agg.MaxLevel,
                        agg.AvgLevel,
                        agg.MinLevel,
                        agg.DurationHours,
                        agg.ReadingCount,
                        agg.CalculatedAt
                    },
                    ct);
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}

