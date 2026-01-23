using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat71_GetUserSubscription.DTOs
{
    public class GetUserSubscriptionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserPlanSubscriptionDto? Subscription { get; set; }
    }
}