using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.Extensions.Logging;

namespace FDAAPI.Infra.Services.Notifications
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly IAlertTemplateRepository _repository;
        private readonly ILogger<NotificationTemplateService> _logger;
        private bool _templatesChecked = false;
        private bool _hasTemplates = false;

        public NotificationTemplateService(
            IAlertTemplateRepository repository,
            ILogger<NotificationTemplateService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public bool HasTemplates()
        {
            if (!_templatesChecked)
            {
                // Check once at startup
                var templates = _repository.GetAllAsync(isActive: true).GetAwaiter().GetResult();
                _hasTemplates = templates.Any();
                _templatesChecked = true;
                _logger.LogInformation("Alert templates available: {HasTemplates}", _hasTemplates);
            }
            return _hasTemplates;
        }

        public string GenerateTitle(Alert alert, NotificationPriority priority)
        {
            // Try to get from DB first
            if (HasTemplates())
            {
                var channel = GetChannelFromPriority(priority);
                var template = _repository.GetByChannelAndSeverityAsync(channel, alert.Severity).GetAwaiter().GetResult();

                if (template != null)
                {
                    return RenderTemplate(template.TitleTemplate, alert);
                }
            }

            // Fallback to hardcoded
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
            // Try to get from DB first
            if (HasTemplates())
            {
                var channelStr = channel.ToString();
                var template = _repository.GetByChannelAndSeverityAsync(channelStr, alert.Severity).GetAwaiter().GetResult();

                if (template != null)
                {
                    return RenderTemplate(template.BodyTemplate, alert, station);
                }
            }

            // Fallback to hardcoded
            if (channel == NotificationChannel.SMS)
            {
                return GenerateSmsContent(alert, station);
            }

            var hardcodedTemplate = $@"
**{station.Name}** is experiencing {alert.Severity} flooding conditions.

Current Water Level: {alert.CurrentValue:F2} cm
Measured At: {alert.TriggeredAt:yyyy-MM-dd HH:mm}

{GetActionableAdvice(alert.Severity)}

Stay safe! - FloodGuard Team
";

            return hardcodedTemplate.Trim();
        }

        public string GenerateSmsContent(Alert alert, Station station)
        {
            // Try to get from DB first
            if (HasTemplates())
            {
                var template = _repository.GetByChannelAndSeverityAsync("SMS", alert.Severity).GetAwaiter().GetResult();

                if (template != null)
                {
                    return RenderTemplate(template.BodyTemplate, alert, station);
                }
            }

            // Fallback to hardcoded
            return $"FLOOD ALERT: {station.Name} - {alert.Severity.ToUpper()} " +
                   $"level {alert.CurrentValue:F1}cm. Check app for details. -FloodGuard";
        }

        private string GetChannelFromPriority(NotificationPriority priority)
        {
            // Map priority to default channel for title lookup
            return priority >= NotificationPriority.High ? "Push" : "InApp";
        }

        private string RenderTemplate(string template, Alert alert, Station? station = null)
        {
            var result = template;

            // Replace alert-related variables
            result = result.Replace("{{station_name}}", station?.Name ?? "Unknown");
            result = result.Replace("{{water_level}}", $"{alert.CurrentValue:F2}m");
            result = result.Replace("{{water_level_raw}}", alert.CurrentValue.ToString("F2"));
            result = result.Replace("{{severity}}", alert.Severity?.ToLower() ?? "unknown");
            result = result.Replace("{{time}}", alert.TriggeredAt.ToString("yyyy-MM-dd HH:mm"));
            result = result.Replace("{{message}}", alert.Message ?? "");
            result = result.Replace("{{threshold}}", alert.AlertRule?.ThresholdValue.ToString("F2") ?? "N/A");

            // Replace station-related variables
            if (station != null)
            {
                result = result.Replace("{{address}}", station.LocationDesc ?? station.RoadName ?? "Unknown");
            }

            return result;
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
