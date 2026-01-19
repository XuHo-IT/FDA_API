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
    public class PgsqlFloodAnalyticsHotspotRepository : IFloodAnalyticsHotspotRepository
    {
        private readonly AppDbContext _context;

        public PgsqlFloodAnalyticsHotspotRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FloodAnalyticsHotspot>> GetTopHotspotsAsync(
            DateTime periodStart,
            DateTime periodEnd,
            int topN,
            CancellationToken ct = default)
        {
            return await _context.FloodAnalyticsHotspots
                .AsNoTracking()
                .Where(h => h.PeriodStart == periodStart && h.PeriodEnd == periodEnd)
                .OrderByDescending(h => h.Score)
                .ThenBy(h => h.Rank)
                .Take(topN)
                .ToListAsync(ct);
        }

        public async Task BulkUpsertAsync(
            List<FloodAnalyticsHotspot> hotspots,
            CancellationToken ct = default)
        {
            if (!hotspots.Any())
                return;

            // Use raw SQL for PostgreSQL ON CONFLICT DO UPDATE (idempotent upsert)
            foreach (var hotspot in hotspots)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO ""FloodAnalyticsHotspots"" 
                        (""Id"", ""AdministrativeAreaId"", ""Score"", ""Rank"", ""PeriodStart"", ""PeriodEnd"", ""CalculatedAt"")
                      VALUES 
                        ({0}, {1}, {2}, {3}, {4}, {5}, {6})
                      ON CONFLICT (""AdministrativeAreaId"", ""PeriodStart"", ""PeriodEnd"")
                      DO UPDATE SET
                        ""Score"" = EXCLUDED.""Score"",
                        ""Rank"" = EXCLUDED.""Rank"",
                        ""CalculatedAt"" = EXCLUDED.""CalculatedAt""",
                    new object[]
                    {
                        hotspot.Id,
                        hotspot.AdministrativeAreaId,
                        hotspot.Score,
                        hotspot.Rank,
                        hotspot.PeriodStart,
                        hotspot.PeriodEnd,
                        hotspot.CalculatedAt
                    },
                    ct);
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}

