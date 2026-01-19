using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG68_DeleteSubscription
{
    public sealed record DeleteSubscriptionRequest(
        Guid SubscriptionId,
        Guid UserId  // For authorization check
    ) : IFeatureRequest<DeleteSubscriptionResponse>;
}