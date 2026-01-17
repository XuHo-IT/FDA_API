using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Notifications
{
    public interface INotificationTemplateService
    {
        string GenerateTitle(Alert alert, NotificationPriority priority);
        string GenerateBody(Alert alert, Station station, NotificationChannel channel);
        string GenerateSmsContent(Alert alert, Station station);
    }
}