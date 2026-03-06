using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Notifications
{
    public interface INotificationTemplateService
    {
        /// <summary>
        /// Generate notification title - uses DB template first, falls back to hardcoded
        /// </summary>
        string GenerateTitle(Alert alert, NotificationPriority priority);

        /// <summary>
        /// Generate notification body - uses DB template first, falls back to hardcoded
        /// </summary>
        string GenerateBody(Alert alert, Station station, NotificationChannel channel);

        /// <summary>
        /// Generate SMS content - uses DB template first, falls back to hardcoded
        /// </summary>
        string GenerateSmsContent(Alert alert, Station station);

        /// <summary>
        /// Check if DB templates are available
        /// </summary>
        bool HasTemplates();
    }
}
