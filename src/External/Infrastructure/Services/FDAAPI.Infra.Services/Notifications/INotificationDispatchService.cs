using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Notifications
{
    public interface INotificationDispatchService
    {
        Task<bool> DispatchNotificationAsync(
            NotificationLog notificationLog,
            User user,
            CancellationToken ct = default);
    }
}