namespace FDAAPI.App.FeatG39_SubscribeToAlerts
{
    public class SubscribeToAlertsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? SubscriptionId { get; set; }
    }
}