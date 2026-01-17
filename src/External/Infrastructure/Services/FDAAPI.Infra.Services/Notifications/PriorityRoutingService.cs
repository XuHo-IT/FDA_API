using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Notifications
{
    public class PriorityRoutingService : IPriorityRoutingService
    {
        public NotificationPriority DeterminePriority(string severity, SubscriptionTier userTier)
        {
            if (userTier == SubscriptionTier.Authority)
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

            return severity.ToLower() switch
            {
                "critical" => NotificationPriority.Medium,
                "warning" => NotificationPriority.Low,
                _ => NotificationPriority.Low
            };
        }

        public List<NotificationChannel> GetChannelsForPriority(NotificationPriority priority, SubscriptionTier userTier)
        {
            var channels = new List<NotificationChannel>
            {
                NotificationChannel.Push,
                NotificationChannel.InApp
            };

            if (userTier >= SubscriptionTier.Premium)
                channels.Add(NotificationChannel.Email);

            if (priority >= NotificationPriority.High && userTier >= SubscriptionTier.Premium)
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
    }
}