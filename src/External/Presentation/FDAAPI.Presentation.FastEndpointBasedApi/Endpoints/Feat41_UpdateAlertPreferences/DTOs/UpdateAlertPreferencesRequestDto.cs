namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat41_UpdateAlertPreferences.DTOs
{
    public class UpdateAlertPreferencesRequestDto
    {
        public Guid SubscriptionId { get; set; }
        public string? MinSeverity { get; set; }
        public bool? EnablePush { get; set; }
        public bool? EnableEmail { get; set; }
        public bool? EnableSms { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
    }
}