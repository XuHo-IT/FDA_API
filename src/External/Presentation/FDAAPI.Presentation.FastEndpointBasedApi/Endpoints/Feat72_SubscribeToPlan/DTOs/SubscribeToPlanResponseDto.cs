using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat72_SubscribeToPlan.DTOs
{
    public class SubscribeToPlanResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PlanSubscriptionDto? Subscription { get; set; }
    }
}