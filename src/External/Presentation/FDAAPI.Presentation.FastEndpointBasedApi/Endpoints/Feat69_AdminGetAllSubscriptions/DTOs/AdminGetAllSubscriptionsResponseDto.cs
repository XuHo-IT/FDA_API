using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat69_AdminGetAllSubscriptions.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat69_AdminGetAllSubscriptions
{
    public class AdminGetAllSubscriptionsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SubscriptionListDataDto? Data { get; set; }
    }
}