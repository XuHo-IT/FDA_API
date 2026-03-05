using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.FeatG72_SubscribeToPlan
{
    public class SubscribeToPlanResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PlanSubscriptionDto? Subscription { get; set; }
    }
}