namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat39_SubscribeToAlerts.DTOs
{
    public class SubscribeToAlertsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? SubscriptionId { get; set; }
    }
}