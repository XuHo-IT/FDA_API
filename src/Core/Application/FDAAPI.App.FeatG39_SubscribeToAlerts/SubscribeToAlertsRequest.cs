using MediatR;

namespace FDAAPI.App.FeatG39_SubscribeToAlerts
{
    public record SubscribeToAlertsRequest(
        Guid UserId,
        Guid? AreaId,
        Guid? StationId,
        string MinSeverity = "warning",
        bool EnablePush = true,
        bool EnableEmail = false,
        bool EnableSms = false,
        TimeSpan? QuietHoursStart = null,
        TimeSpan? QuietHoursEnd = null
    ) : IRequest<SubscribeToAlertsResponse>;
}