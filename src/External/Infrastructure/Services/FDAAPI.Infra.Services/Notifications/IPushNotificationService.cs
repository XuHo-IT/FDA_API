using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Notifications
{
    public interface IPushNotificationService
    {
        Task<bool> SendPushNotificationAsync(
            string deviceToken,
            string title,
            string body,
            NotificationPriority priority,
            Dictionary<string, string>? data = null,
            CancellationToken ct = default);
    }
}