using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlAlertRepository : IAlertRepository
    {
        private readonly AppDbContext _context;

        public PgsqlAlertRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Alerts
                .Include(a => a.Station)
                .Include(a => a.AlertRule)
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<Guid> CreateAsync(Alert entity, CancellationToken ct = default)
        {
            _context.Alerts.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(Alert entity, CancellationToken ct = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<Alert>> GetActiveAlertsByStationAsync(
            Guid stationId,
            CancellationToken ct = default)
        {
            return await _context.Alerts
                .AsNoTracking()
                .Include(a => a.Station)
                .Include(a => a.AlertRule)
                .Where(a => a.StationId == stationId && a.Status == "open")
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Alert>> GetAlertsBySeverityAsync(
            string severity,
            string status = "open",
            CancellationToken ct = default)
        {
            return await _context.Alerts
                .AsNoTracking()
                .Include(a => a.Station)
                .Where(a => a.Severity.ToLower() == severity.ToLower() && a.Status == status)
                .OrderByDescending(a => a.TriggeredAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Alert>> GetUnnotifiedAlertsAsync(
            int take = 100,
            CancellationToken ct = default)
        {
            return await _context.Alerts
                .AsNoTracking()
                .Include(a => a.Station)
                .Include(a => a.AlertRule)
                .Where(a => a.Status == "open" && !a.NotificationLogs!.Any())
                .OrderBy(a => a.TriggeredAt)
                .Take(take)
                .ToListAsync(ct);
        }


        public async Task<int> CountAlertsAsync(
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct = default)
        {
            var query = _context.Alerts.AsNoTracking();

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.TriggeredAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.TriggeredAt <= toDate.Value);
            }

            return await query.CountAsync(ct);
        }

        public async Task<Dictionary<string, int>> CountAlertsBySeverityAsync(
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct = default)
        {
            var query = _context.Alerts.AsNoTracking();

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.TriggeredAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.TriggeredAt <= toDate.Value);
            }

            return await query
                .GroupBy(a => a.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Severity, x => x.Count, ct);
        }

        public async Task<Dictionary<string, int>> CountAlertsByStatusAsync(
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct = default)
        {
            var query = _context.Alerts.AsNoTracking();

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.TriggeredAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.TriggeredAt <= toDate.Value);
            }

            return await query
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, ct);
        }
    }
}