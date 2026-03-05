using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.FeatG67_GetMySubscriptions
{
    public class GetMySubscriptionsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<UserSubscriptionDto> Subscriptions { get; set; } = new();
        public int TotalCount { get; set; }
    }
}