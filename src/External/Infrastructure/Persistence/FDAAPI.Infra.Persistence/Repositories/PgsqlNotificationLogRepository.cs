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

        public async Task<IEnumerable<NotificationLog>> GetPendingNotificationsAsync(
            int maxRetries = 3,
            CancellationToken ct = default)
        {
            return await _context.NotificationLogs
                .AsNoTracking()
                .Include(n => n.Alert)
                .Include(n => n.User)
                .Where(n => n.Status == "pending" && n.RetryCount < maxRetries)
                .OrderBy(n => n.CreatedAt)
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
    }
}