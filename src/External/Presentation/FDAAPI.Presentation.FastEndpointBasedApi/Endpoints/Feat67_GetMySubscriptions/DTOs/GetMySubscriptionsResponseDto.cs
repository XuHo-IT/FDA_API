using FDAAPI.App.Common.DTOs;
using FDAAPI.App.FeatG67_GetMySubscriptions;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat67_GetMySubscriptions.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat67_GetMySubscriptions
{
    public class GetMySubscriptionsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SubscriptionListDto? Data { get; set; }
    }
}