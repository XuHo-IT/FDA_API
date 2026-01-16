using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Notifications
{
    public interface IPriorityRoutingService
    {
        NotificationPriority DeterminePriority(string severity, SubscriptionTier userTier);
        List<NotificationChannel> GetChannelsForPriority(NotificationPriority priority, SubscriptionTier userTier);
        bool ShouldNotifyUser(string severity, SubscriptionTier userTier, string minSeverity);
    }
}