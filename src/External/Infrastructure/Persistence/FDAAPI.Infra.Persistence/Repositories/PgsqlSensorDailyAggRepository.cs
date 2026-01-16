using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlSensorDailyAggRepository : ISensorDailyAggRepository
    {
        private readonly AppDbContext _context;

        public PgsqlSensorDailyAggRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SensorDailyAgg>> GetByStationAndRangeAsync(
            Guid stationId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default)
        {
            return await _context.SensorDailyAggs
                .AsNoTracking()
                .Where(a => a.StationId == stationId
                    && a.Date >= startDate
                    && a.Date <= endDate)
                .OrderBy(a => a.Date)
                .ToListAsync(ct);
        }

        public async Task<List<SensorDailyAgg>> GetByStationsAndRangeAsync(
            List<Guid> stationIds,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default)
        {
            return await _context.SensorDailyAggs
                .AsNoTracking()
                .Where(a => stationIds.Contains(a.StationId)
                    && a.Date >= startDate
                    && a.Date <= endDate)
                .OrderBy(a => a.StationId)
                .ThenBy(a => a.Date)
                .ToListAsync(ct);
        }

        public async Task<SensorDailyAgg?> GetByStationAndDateAsync(
            Guid stationId,
            DateOnly date,
            CancellationToken ct = default)
        {
            return await _context.SensorDailyAggs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.StationId == stationId
                    && a.Date == date, ct);
        }

        public async Task<Guid> CreateAsync(SensorDailyAgg aggregate, CancellationToken ct = default)
        {
            aggregate.Id = Guid.NewGuid();
            aggregate.CreatedAt = DateTime.UtcNow;
            _context.SensorDailyAggs.Add(aggregate);
            await _context.SaveChangesAsync(ct);
            return aggregate.Id;
        }

        public async Task BulkInsertAsync(IEnumerable<SensorDailyAgg> aggregates, CancellationToken ct = default)
        {
            foreach (var agg in aggregates)
            {
                agg.Id = Guid.NewGuid();
                agg.CreatedAt = DateTime.UtcNow;
            }
            await _context.SensorDailyAggs.AddRangeAsync(aggregates, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<(decimal MaxLevel, decimal MinLevel, decimal AvgLevel, int TotalFloodHours, int TotalReadings)>
            GetStatisticsSummaryAsync(
                Guid stationId,
                DateOnly startDate,
                DateOnly endDate,
                CancellationToken ct = default)
        {
            var data = await _context.SensorDailyAggs
                .AsNoTracking()
                .Where(a => a.StationId == stationId
                    && a.Date >= startDate
                    && a.Date <= endDate)
                .ToListAsync(ct);

            if (!data.Any())
                return (0, 0, 0, 0, 0);

            return (
                MaxLevel: data.Max(d => d.MaxLevel),
                MinLevel: data.Min(d => d.MinLevel),
                AvgLevel: data.Average(d => d.AvgLevel),
                TotalFloodHours: data.Sum(d => d.FloodHours),
                TotalReadings: data.Sum(d => d.ReadingCount)
            );
        }
    }
}
