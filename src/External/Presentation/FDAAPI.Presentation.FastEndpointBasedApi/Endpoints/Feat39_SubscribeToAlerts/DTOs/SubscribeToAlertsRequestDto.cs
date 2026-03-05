namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat39_SubscribeToAlerts.DTOs
{
    public class SubscribeToAlertsRequestDto
    {
        public Guid? AreaId { get; set; }
        public Guid? StationId { get; set; }
        public string MinSeverity { get; set; } = "warning";
        public bool EnablePush { get; set; } = true;
        public bool EnableEmail { get; set; } = false;
        public bool EnableSms { get; set; } = false;
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
    }
}