using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat67_GetMySubscriptions.DTOs
{
    public class SubscriptionListDto
    {
        public List<UserSubscriptionDto> Subscriptions { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
