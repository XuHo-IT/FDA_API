using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Notifications
{
    public class PriorityRoutingService : IPriorityRoutingService
    {
        public NotificationPriority DeterminePriority(string severity, SubscriptionTier userTier)
        {
            if (userTier == SubscriptionTier.Monitor)
            {
                return severity.ToLower() switch
                {
                    "critical" => NotificationPriority.Critical,
                    "warning" => NotificationPriority.High,
                    _ => NotificationPriority.Medium
                };
            }

            if (userTier == SubscriptionTier.Premium)
            {
                return severity.ToLower() switch
                {
                    "critical" => NotificationPriority.High,
                    "warning" => NotificationPriority.Medium,
                    _ => NotificationPriority.Low
                };
            }

            // Free users
            return severity.ToLower() switch
            {
                "critical" => NotificationPriority.Medium,
                "warning" => NotificationPriority.Low,
                _ => NotificationPriority.Low
            };
        }

        public List<NotificationChannel> GetAvailableChannelsForTier(SubscriptionTier userTier)
        {
            var channels = new List<NotificationChannel>
            {
                NotificationChannel.Push,
                NotificationChannel.InApp
            };

            // Free users can use Email
            if (userTier >= SubscriptionTier.Free)
                channels.Add(NotificationChannel.Email);

            // Premium and Monitor can use SMS
            if (userTier >= SubscriptionTier.Premium)
                channels.Add(NotificationChannel.SMS);

            return channels;
        }

        public bool ShouldNotifyUser(string severity, SubscriptionTier userTier, string minSeverity)
        {
            var severityLevels = new Dictionary<string, int>
            {
                ["info"] = 0,
                ["caution"] = 1,
                ["warning"] = 2,
                ["critical"] = 3
            };

            var currentLevel = severityLevels.GetValueOrDefault(severity.ToLower(), 0);
            var minLevel = severityLevels.GetValueOrDefault(minSeverity.ToLower(), 0);

            return currentLevel >= minLevel;
        }

        public int GetDispatchDelaySeconds(SubscriptionTier userTier, NotificationPriority priority)
        {
            // Monitor - immediate for high priority, 10s for low
            if (userTier == SubscriptionTier.Monitor)
            {
                return priority >= NotificationPriority.High ? 0 : 10;
            }

            // Premium - immediate for high priority, 20s for low
            if (userTier == SubscriptionTier.Premium)
            {
                return priority >= NotificationPriority.High ? 0 : 20;
            }

            // Free - 60s for high priority, 120s for low
            return priority >= NotificationPriority.High ? 60 : 120;
        }

        public int GetMaxRetriesForTier(SubscriptionTier userTier, NotificationPriority priority)
        {
            if (userTier == SubscriptionTier.Monitor && priority >= NotificationPriority.High)
                return 5; // More retries for critical monitoring alerts

            if (userTier == SubscriptionTier.Premium)
                return 3;

            // Free users - limited retries
            return priority >= NotificationPriority.High ? 1 : 0;
        }
    }
}