using MediatR;

namespace FDAAPI.App.FeatG41_UpdateAlertPreferences
{
    public record UpdateAlertPreferencesRequest(
        Guid AreaId,
        Guid UserId,
        string? MinSeverity = null,
        bool? EnablePush = null,
        bool? EnableEmail = null,
        bool? EnableSms = null,
        TimeSpan? QuietHoursStart = null,
        TimeSpan? QuietHoursEnd = null
    ) : IRequest<UpdateAlertPreferencesResponse>;
}