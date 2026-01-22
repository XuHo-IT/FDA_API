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
        Task<List<NotificationLog>> GetPendingAndRetryNotificationsAsync(
            int limit,
            CancellationToken ct = default);

        Task<int> CountNotificationsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
        Task<int> CountNotificationsByStatusAsync(string status, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
        Task<Dictionary<string, (int Sent, int Failed)>> GetNotificationStatsByChannelAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
        Task<double> GetAverageDeliveryTimeAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    }
}