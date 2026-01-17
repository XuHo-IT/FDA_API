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
    public class PgsqlSensorHourlyAggRepository : ISensorHourlyAggRepository
    {
        private readonly AppDbContext _context;

        public PgsqlSensorHourlyAggRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SensorHourlyAgg>> GetByStationAndRangeAsync(
            Guid stationId,
            DateTime startHour,
            DateTime endHour,
            CancellationToken ct = default)
        {
            return await _context.SensorHourlyAggs
                .AsNoTracking()
                .Where(a => a.StationId == stationId
                    && a.HourStart >= startHour
                    && a.HourStart <= endHour)
                .OrderBy(a => a.HourStart)
                .ToListAsync(ct);
        }

        public async Task<List<SensorHourlyAgg>> GetByStationsAndRangeAsync(
            List<Guid> stationIds,
            DateTime startHour,
            DateTime endHour,
            CancellationToken ct = default)
        {
            return await _context.SensorHourlyAggs.AsNoTracking()
                .Where(a => stationIds.Contains(a.StationId)
                    && a.HourStart >= startHour
                    && a.HourStart <= endHour)
                .OrderBy(a => a.StationId)
                .ThenBy(a => a.HourStart)
                .ToListAsync(ct);
        }

        public async Task<SensorHourlyAgg?> GetByStationAndHourAsync(
            Guid stationId,
            DateTime hourStart,
            CancellationToken ct = default)
        {
            return await _context.SensorHourlyAggs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.StationId == stationId
                    && a.HourStart == hourStart, ct);
        }

        public async Task<Guid> CreateAsync(SensorHourlyAgg aggregate, CancellationToken ct = default)
        {
            aggregate.Id = Guid.NewGuid();
            aggregate.CreatedAt = DateTime.UtcNow;
            _context.SensorHourlyAggs.Add(aggregate);
            await _context.SaveChangesAsync(ct);
            return aggregate.Id;
        }

        public async Task BulkInsertAsync(IEnumerable<SensorHourlyAgg> aggregates, CancellationToken ct = default)
        {
            foreach (var agg in aggregates)
            {
                agg.Id = Guid.NewGuid();
                agg.CreatedAt = DateTime.UtcNow;
            }
            await _context.SensorHourlyAggs.AddRangeAsync(aggregates, ct);
            await _context.SaveChangesAsync(ct);
        }
    }
}
