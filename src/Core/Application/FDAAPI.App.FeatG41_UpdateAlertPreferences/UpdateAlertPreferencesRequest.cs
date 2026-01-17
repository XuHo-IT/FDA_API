using MediatR;

namespace FDAAPI.App.FeatG41_UpdateAlertPreferences
{
    public record UpdateAlertPreferencesRequest(
        Guid SubscriptionId,
        Guid UserId,                    // For authorization check
        string? MinSeverity = null,     // Optional updates
        bool? EnablePush = null,
        bool? EnableEmail = null,
        bool? EnableSms = null,
        TimeSpan? QuietHoursStart = null,
        TimeSpan? QuietHoursEnd = null
    ) : IRequest<UpdateAlertPreferencesResponse>;
}