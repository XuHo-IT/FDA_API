using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat69_AdminGetAllSubscriptions.DTOs
{
    public class SubscriptionListDataDto
    {
        public List<AdminSubscriptionDto> Subscriptions { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new();
    }
}
