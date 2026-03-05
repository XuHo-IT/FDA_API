using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat73_CancelPlanSubscription.DTOs
{
    public class CancelSubscriptionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CancelledPlanSubscriptionDto? CancelledSubscription { get; set; }
    }
}
