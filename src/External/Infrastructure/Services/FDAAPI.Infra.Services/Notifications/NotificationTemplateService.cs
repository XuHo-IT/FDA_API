using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Notifications
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        public string GenerateTitle(Alert alert, NotificationPriority priority)
        {
            var urgency = priority switch
            {
                NotificationPriority.Critical => "CRITICAL",
                NotificationPriority.High => "WARNING",
                NotificationPriority.Medium => "ALERT",
                _ => "Update"
            };

            return $"{urgency}: Flood Alert - {alert.Severity.ToUpper()}";
        }

        public string GenerateBody(Alert alert, Station station, NotificationChannel channel)
        {
            if (channel == NotificationChannel.SMS)
            {
                return GenerateSmsContent(alert, station);
            }

            var template = $@"
            **{station.Name}** is experiencing {alert.Severity} flooding conditions.

            Current Water Level: {alert.CurrentValue:F2} cm
            Measured At: {alert.TriggeredAt:yyyy-MM-dd HH:mm}

            {GetActionableAdvice(alert.Severity)}

            Stay safe! - FloodGuard Team
            ";

            return template.Trim();
        }

        public string GenerateSmsContent(Alert alert, Station station)
        {
            // SMS must be concise (<160 chars ideally)
            return $"FLOOD ALERT: {station.Name} - {alert.Severity.ToUpper()} " +
                   $"level {alert.CurrentValue:F1}cm. Check app for details. -FloodGuard";
        }

        private string GetActionableAdvice(string severity)
        {
            return severity.ToLower() switch
            {
                "critical" => "⚠️ EVACUATE IMMEDIATELY if in affected area. Follow local authority instructions.",
                "warning" => "⚠️ Prepare to evacuate. Monitor conditions closely.",
                "caution" => "ℹ️ Be alert and avoid affected areas if possible.",
                _ => "ℹ️ Monitor the situation."
            };
        }
    }
}