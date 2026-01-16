namespace FDAAPI.App.FeatG43_DispatchNotifications
{
    public class DispatchNotificationsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int NotificationsCreated { get; set; }
        public int NotificationsSent { get; set; }
        public int NotificationsFailed { get; set; }
        public int AlertsProcessed { get; set; }
    }
}