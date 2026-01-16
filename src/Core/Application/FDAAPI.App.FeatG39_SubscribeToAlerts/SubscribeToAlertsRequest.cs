using MediatR;

namespace FDAAPI.App.FeatG39_SubscribeToAlerts
{
    public record SubscribeToAlertsRequest(
        Guid UserId,
        Guid? StationId,
        Guid? AreaId,
        string MinSeverity = "warning",
        bool EnablePush = true,
        bool EnableEmail = false,
        bool EnableSms = false
    ) : IRequest<SubscribeToAlertsResponse>;
}