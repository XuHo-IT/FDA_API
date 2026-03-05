using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.FeatG69_AdminGetAllSubscriptions
{
    public class AdminGetAllSubscriptionsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AdminSubscriptionDto> Subscriptions { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new();
    }
}