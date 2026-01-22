using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlNotificationLogRepository : INotificationLogRepository
    {
        private readonly AppDbContext _context;

        public PgsqlNotificationLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(NotificationLog entity, CancellationToken ct = default)
        {
            _context.NotificationLogs.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(NotificationLog entity, CancellationToken ct = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(
            int limit,
            CancellationToken ct = default)
        {
            return await _context.NotificationLogs
                .Where(n =>
                    (n.Status == "pending" && n.UpdatedAt <= DateTime.UtcNow) || // Respect delay
                    (n.Status == "pending_retry" && n.UpdatedAt <= DateTime.UtcNow)
                )
                .OrderByDescending(n => n.Priority)
                .ThenBy(n => n.CreatedAt)
                .Take(limit)
                .Include(n => n.User)
                .Include(n => n.Alert)
                    .ThenInclude(a => a!.Station)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<NotificationLog>> GetByUserIdAsync(
            Guid userId,
            int skip = 0,
            int take = 50,
            CancellationToken ct = default)
        {
            return await _context.NotificationLogs
                .AsNoTracking()
                .Include(n => n.Alert)
                    .ThenInclude(a => a!.Station)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<bool> IsUserNotifiedAsync(
            Guid userId,
            Guid alertId,
            CancellationToken ct = default)
        {
            return await _context.NotificationLogs
                .AsNoTracking()
                .AnyAsync(n => n.UserId == userId && n.AlertId == alertId, ct);
        }

        public async Task<IEnumerable<NotificationLog>> GetPendingNotificationsAsync(
            int maxRetries = 3,
            CancellationToken ct = default)
        {
            return await _context.NotificationLogs
                .AsNoTracking()
                .Include(n => n.Alert)
                .Include(n => n.User)
                .Where(n => n.Status == "pending" &&
                            n.RetryCount < maxRetries &&
                            n.UpdatedAt <= DateTime.UtcNow) // Check delay
                .OrderByDescending(n => n.Priority) // PRIORITY FIRST
                .ThenBy(n => n.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<int> CountNotificationsAsync(
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct = default)
        {
            var query = _context.NotificationLogs.AsNoTracking();

            if (fromDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= toDate.Value);
            }

            return await query.CountAsync(ct);
        }

        public async Task<int> CountNotificationsByStatusAsync(
            string status,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct = default)
        {
            var query = _context.NotificationLogs
                .AsNoTracking()
                .Where(n => n.Status == status);

            if (fromDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= toDate.Value);
            }

            return await query.CountAsync(ct);
        }

        public async Task<Dictionary<string, (int Sent, int Failed)>> GetNotificationStatsByChannelAsync(
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct = default)
        {
            var query = _context.NotificationLogs.AsNoTracking();

            if (fromDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= toDate.Value);
            }

            var stats = await query
                .GroupBy(n => n.Channel)
                .Select(g => new
                {
                    Channel = g.Key.ToString(),
                    Sent = g.Count(n => n.Status == "sent"),
                    Failed = g.Count(n => n.Status == "failed")
                })
                .ToListAsync(ct);

            return stats.ToDictionary(
                x => x.Channel,
                x => (x.Sent, x.Failed)
            );
        }

        public async Task<double> GetAverageDeliveryTimeAsync(
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct = default)
        {
            var query = _context.NotificationLogs
                .AsNoTracking()
                .Where(n => n.Status == "sent" && n.SentAt.HasValue && n.DeliveredAt.HasValue);

            if (fromDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= toDate.Value);
            }

            var deliveryTimes = await query
                .Select(n => (n.DeliveredAt!.Value - n.SentAt!.Value).TotalSeconds)
                .ToListAsync(ct);

            return deliveryTimes.Any() ? deliveryTimes.Average() : 0;
        }
    }
}