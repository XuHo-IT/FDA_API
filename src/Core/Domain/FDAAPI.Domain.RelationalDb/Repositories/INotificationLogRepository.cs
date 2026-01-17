using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface INotificationLogRepository
    {
        Task<Guid> CreateAsync(NotificationLog entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(NotificationLog entity, CancellationToken ct = default);

        // Get pending notifications for retry
        Task<IEnumerable<NotificationLog>> GetPendingNotificationsAsync(
            int maxRetries = 3,
            CancellationToken ct = default);

        // Get notification history for user
        Task<IEnumerable<NotificationLog>> GetByUserIdAsync(
            Guid userId,
            int skip = 0,
            int take = 50,
            CancellationToken ct = default);

        // Check if user already notified for this alert
        Task<bool> IsUserNotifiedAsync(
            Guid userId,
            Guid alertId,
            CancellationToken ct = default);
    }
}