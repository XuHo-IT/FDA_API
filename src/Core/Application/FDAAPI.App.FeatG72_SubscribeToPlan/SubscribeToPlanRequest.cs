using MediatR;

namespace FDAAPI.App.FeatG72_SubscribeToPlan
{
    /// <summary>
    /// Request to subscribe user to a pricing plan
    /// </summary>
    public record SubscribeToPlanRequest(
        Guid UserId,
        string PlanCode, // "FREE", "PREMIUM", "MONITOR"
        int DurationMonths = 12 // Default 1 year
    ) : IRequest<SubscribeToPlanResponse>;
}