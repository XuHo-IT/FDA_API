using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG67_GetMySubscriptions
{
    public sealed record GetMySubscriptionsRequest(
        Guid UserId
    ) : IFeatureRequest<GetMySubscriptionsResponse>;
}