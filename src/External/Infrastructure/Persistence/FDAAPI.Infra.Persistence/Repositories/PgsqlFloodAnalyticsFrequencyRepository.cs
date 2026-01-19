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
    public class PgsqlFloodAnalyticsFrequencyRepository : IFloodAnalyticsFrequencyRepository
    {
        private readonly AppDbContext _context;

        public PgsqlFloodAnalyticsFrequencyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FloodAnalyticsFrequency>> GetByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            string bucketType,
            CancellationToken ct = default)
        {
            return await _context.FloodAnalyticsFrequencies
                .AsNoTracking()
                .Where(f => f.AdministrativeAreaId == administrativeAreaId
                    && f.BucketType == bucketType
                    && f.TimeBucket >= startDate
                    && f.TimeBucket < endDate)
                .OrderBy(f => f.TimeBucket)
                .ToListAsync(ct);
        }

        public async Task<FloodAnalyticsFrequency?> GetByAdministrativeAreaBucketAsync(
            Guid administrativeAreaId,
            DateTime timeBucket,
            string bucketType,
            CancellationToken ct = default)
        {
            return await _context.FloodAnalyticsFrequencies
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.AdministrativeAreaId == administrativeAreaId
                    && f.TimeBucket == timeBucket
                    && f.BucketType == bucketType, ct);
        }

        public async Task BulkUpsertAsync(
            List<FloodAnalyticsFrequency> aggregates,
            CancellationToken ct = default)
        {
            if (!aggregates.Any())
                return;

            // Use raw SQL for PostgreSQL ON CONFLICT DO UPDATE (idempotent upsert)
            foreach (var agg in aggregates)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO ""FloodAnalyticsFrequency"" 
                        (""Id"", ""AdministrativeAreaId"", ""TimeBucket"", ""BucketType"", ""EventCount"", ""ExceedCount"", ""CalculatedAt"")
                      VALUES 
                        ({0}, {1}, {2}, {3}, {4}, {5}, {6})
                      ON CONFLICT (""AdministrativeAreaId"", ""TimeBucket"", ""BucketType"")
                      DO UPDATE SET
                        ""EventCount"" = EXCLUDED.""EventCount"",
                        ""ExceedCount"" = EXCLUDED.""ExceedCount"",
                        ""CalculatedAt"" = EXCLUDED.""CalculatedAt""",
                    new object[]
                    {
                        agg.Id,
                        agg.AdministrativeAreaId,
                        agg.TimeBucket,
                        agg.BucketType,
                        agg.EventCount,
                        agg.ExceedCount,
                        agg.CalculatedAt
                    },
                    ct);
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}

