namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat69_AdminGetAllSubscriptions
{
    public class AdminGetAllSubscriptionsRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public Guid? UserId { get; set; }
        public Guid? StationId { get; set; }
    }
}