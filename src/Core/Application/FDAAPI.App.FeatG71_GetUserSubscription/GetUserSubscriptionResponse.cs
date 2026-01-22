using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.FeatG71_GetUserSubscription
{
    public class GetUserSubscriptionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserPlanSubscriptionDto? Subscription { get; set; }
    }
}