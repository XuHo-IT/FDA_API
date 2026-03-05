namespace FDAAPI.App.FeatG42_ProcessAlerts
{
    public class ProcessAlertsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AlertsCreated { get; set; }
        public int AlertsUpdated { get; set; }
        public int AlertsPending { get; set; }
    }
}