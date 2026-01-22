using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Notifications
{
    public interface IPriorityRoutingService
    {
        NotificationPriority DeterminePriority(string severity, SubscriptionTier userTier);
        List<NotificationChannel> GetAvailableChannelsForTier(SubscriptionTier userTier);
        bool ShouldNotifyUser(string severity, SubscriptionTier userTier, string minSeverity);
        int GetDispatchDelaySeconds(SubscriptionTier userTier, NotificationPriority priority);
        int GetMaxRetriesForTier(SubscriptionTier userTier, NotificationPriority priority);
    }
}