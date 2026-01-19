using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG69_AdminGetAllSubscriptions
{
    public sealed record AdminGetAllSubscriptionsRequest(
        int Page = 1,
        int PageSize = 50,
        Guid? UserId = null,
        Guid? StationId = null
    ) : IFeatureRequest<AdminGetAllSubscriptionsResponse>;
}