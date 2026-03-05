using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using Microsoft.Extensions.Logging;

namespace FDAAPI.Infra.Services.Notifications
{
    public class NotificationDispatchService : INotificationDispatchService
    {
        private readonly IPushNotificationService _pushService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<NotificationDispatchService> _logger;

        public NotificationDispatchService(
            IPushNotificationService pushService,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<NotificationDispatchService> logger)
        {
            _pushService = pushService;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<bool> DispatchNotificationAsync(
            NotificationLog notificationLog,
            User user,
            CancellationToken ct = default)
        {
            bool success = false;

            try
            {
                switch (notificationLog.Channel)
                {
                    case NotificationChannel.Push:
                        // Destination should be device token
                        success = await _pushService.SendPushNotificationAsync(
                            notificationLog.Destination,
                            "Flood Alert",
                            notificationLog.Content,
                            notificationLog.Priority,
                            null,
                            ct);
                        break;

                    case NotificationChannel.Email:
                        success = await _emailService.SendEmailAsync(
                            notificationLog.Destination,
                            "Flood Alert Notification",
                            notificationLog.Content,
                            ct);
                        break;

                    case NotificationChannel.SMS:
                        success = await _smsService.SendSmsAsync(
                            notificationLog.Destination,
                            notificationLog.Content,
                            ct);
                        break;

                    case NotificationChannel.InApp:
                        // In-app notifications are just stored in DB, no external dispatch needed
                        success = true;
                        break;

                    default:
                        _logger.LogWarning("Unknown notification channel: {Channel}", notificationLog.Channel);
                        break;
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching notification via {Channel}", notificationLog.Channel);
                return false;
            }
        }
    }
}