using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.FeatG73_CancelSubscription
{
    public class CancelSubscriptionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CancelledPlanSubscriptionDto? CancelledSubscription { get; set; }
    }
}