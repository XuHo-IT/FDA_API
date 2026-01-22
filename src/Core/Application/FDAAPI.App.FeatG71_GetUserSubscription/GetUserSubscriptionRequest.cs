using MediatR;

namespace FDAAPI.App.FeatG71_GetUserSubscription
{
    public record GetUserSubscriptionRequest(Guid UserId) : IRequest<GetUserSubscriptionResponse>;
}