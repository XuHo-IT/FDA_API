using MediatR;

namespace FDAAPI.App.FeatG73_CancelSubscription
{
    /// <summary>
    /// Request to cancel current subscription and return to Free tier
    /// </summary>
    public record CancelSubscriptionRequest(
        Guid UserId,
        string? CancelReason = null
    ) : IRequest<CancelSubscriptionResponse>;
}