using MediatR;

namespace FDAAPI.App.FeatG71_UpdateAlertPreferences
{
    public sealed record UpdateAlertPreferencesRequest(
        Guid SubscriptionId,
        Guid UserId,
        string? MinSeverity = null,
        bool? EnablePush = null,
        bool? EnableEmail = null,
        bool? EnableSms = null,
        TimeSpan? QuietHoursStart = null,
        TimeSpan? QuietHoursEnd = null
    ) : IRequest<UpdateAlertPreferencesResponse>;
}