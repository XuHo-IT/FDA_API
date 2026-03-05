using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlSensorReadingRepository : ISensorReadingRepository
    {
        private readonly AppDbContext _context;

        public PgsqlSensorReadingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SensorReading?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.SensorReadings
                .Include(sr => sr.Station)
                .FirstOrDefaultAsync(sr => sr.Id == id, ct);
        }

        public async Task<Guid> CreateAsync(SensorReading entity, CancellationToken ct = default)
        {
            _context.SensorReadings.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(SensorReading entity, CancellationToken ct = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var sensorReading = await _context.SensorReadings.FindAsync(new object[] { id }, ct);
            if (sensorReading == null)
            {
                return false;
            }

            _context.SensorReadings.Remove(sensorReading);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<SensorReading>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default)
        {
            return await _context.SensorReadings
                .Include(sr => sr.Station)
                .OrderByDescending(sr => sr.MeasuredAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<(IEnumerable<SensorReading> Readings, int TotalCount)> GetSensorReadingsAsync(
            Guid? stationId,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _context.SensorReadings.AsNoTracking().AsQueryable();

            if (stationId.HasValue)
            {
                query = query.Where(sr => sr.StationId == stationId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(sr => sr.MeasuredAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(sr => sr.MeasuredAt <= endDate.Value);
            }

            int totalCount = await query.CountAsync(ct);

            var readings = await query
                .OrderByDescending(sr => sr.MeasuredAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (readings, totalCount);
        }

        public async Task<List<SensorReading>> GetLatestReadingsByStationsAsync(
            IEnumerable<Guid> stationIds,
            CancellationToken ct = default)
        {
            return await _context.SensorReadings
                .AsNoTracking()
                .Where(sr => stationIds.Contains(sr.StationId))
                .GroupBy(sr => sr.StationId)
                .Select(g => g.OrderByDescending(sr => sr.MeasuredAt).First())
                .ToListAsync(ct);
        }

        public async Task<List<SensorReading>> GetByStationAndTimeRangeAsync(
            Guid stationId,
            DateTime startDate,
            DateTime endDate,
            int limit = 1000,
            CancellationToken ct = default)
        {
            return await _context.SensorReadings
                .AsNoTracking()
                .Where(sr => sr.StationId == stationId
                    && sr.MeasuredAt >= startDate
                    && sr.MeasuredAt <= endDate)
                .OrderBy(sr => sr.MeasuredAt)
                .Take(limit)
                .ToListAsync(ct);
        }

        public async Task<List<SensorReading>> GetByHourAsync(
            DateTime hour,
            CancellationToken ct = default)
        {
            return await _context.SensorReadings
                .AsNoTracking()
                .Where(sr => sr.MeasuredAt >= hour && sr.MeasuredAt < hour.AddHours(1))
                .OrderBy(sr => sr.MeasuredAt)
                .ToListAsync(ct);
        }

        public async Task<int> CountExceedancesByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            // Count readings where value >= threshold_warning (exceedance = warning or critical)
            // Join with Station to get thresholds and filter by administrative area
            return await _context.SensorReadings
                .AsNoTracking()
                .Join(_context.Stations,
                    sr => sr.StationId,
                    s => s.Id,
                    (sr, s) => new { Reading = sr, Station = s })
                .Where(x => x.Station.AdministrativeAreaId == administrativeAreaId
                    && x.Reading.MeasuredAt >= startDate
                    && x.Reading.MeasuredAt < endDate
                    && x.Station.ThresholdWarning.HasValue
                    && x.Reading.Value >= (double)x.Station.ThresholdWarning.Value) // Value >= threshold_warning = exceedance
                .CountAsync(ct);
        }

        public async Task<List<SensorReading>> GetByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            // Get stations in the administrative area
            var stationIds = await _context.Stations
                .Where(s => s.AdministrativeAreaId == administrativeAreaId)
                .Select(s => s.Id)
                .ToListAsync(ct);

            if (!stationIds.Any())
                return new List<SensorReading>();

            return await _context.SensorReadings
                .AsNoTracking()
                .Where(sr => stationIds.Contains(sr.StationId)
                    && sr.MeasuredAt >= startDate
                    && sr.MeasuredAt < endDate)
                .OrderBy(sr => sr.MeasuredAt)
                .ToListAsync(ct);
        }

        public async Task<List<(SensorReading Reading, Station Station)>> GetReadingsWithStationsByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            // Get readings with station info (for threshold comparison)
            var query = from sr in _context.SensorReadings
                       join s in _context.Stations on sr.StationId equals s.Id
                       where s.AdministrativeAreaId == administrativeAreaId
                          && sr.MeasuredAt >= startDate
                          && sr.MeasuredAt < endDate
                       orderby sr.MeasuredAt
                       select new { Reading = sr, Station = s };

            var results = await query
                .AsNoTracking()
                .ToListAsync(ct);

            return results.Select(x => (x.Reading, x.Station)).ToList();
        }
    }
}