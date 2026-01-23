using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG73_CancelSubscription
{
    /// <summary>
    /// Handler to cancel current subscription and return user to Free tier
    /// </summary>
    public class CancelSubscriptionHandler : IRequestHandler<CancelSubscriptionRequest, CancelSubscriptionResponse>
    {
        private readonly IUserSubscriptionRepository _subscriptionRepo;

        public CancelSubscriptionHandler(IUserSubscriptionRepository subscriptionRepo)
        {
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<CancelSubscriptionResponse> Handle(
            CancelSubscriptionRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Get active subscription
                var activeSubscription = await _subscriptionRepo.GetActiveSubscriptionAsync(request.UserId, ct);

                if (activeSubscription == null)
                {
                    return new CancelSubscriptionResponse
                    {
                        Success = false,
                        Message = "No active subscription found to cancel"
                    };
                }

                // 2. Don't allow cancelling FREE plan (it's the default)
                if (activeSubscription.Plan?.Code == "FREE")
                {
                    return new CancelSubscriptionResponse
                    {
                        Success = false,
                        Message = "Cannot cancel Free plan. You are already on the Free tier."
                    };
                }

                // 3. Cancel subscription
                var previousPlanName = activeSubscription.Plan?.Name ?? "Unknown";
                var previousTier = activeSubscription.Plan?.Tier.ToString() ?? "Unknown";

                activeSubscription.Status = "cancelled";
                activeSubscription.EndDate = DateTime.UtcNow;
                activeSubscription.CancelReason = request.CancelReason ?? "User requested cancellation";
                activeSubscription.UpdatedBy = request.UserId;
                activeSubscription.UpdatedAt = DateTime.UtcNow;

                await _subscriptionRepo.UpdateAsync(activeSubscription, ct);

                return new CancelSubscriptionResponse
                {
                    Success = true,
                    Message = $"Successfully cancelled {previousPlanName}. You are now on the Free tier.",
                    CancelledSubscription = new CancelledPlanSubscriptionDto
                    {
                        SubscriptionId = activeSubscription.Id,
                        PlanName = previousPlanName,
                        PreviousTier = previousTier,
                        CancelledAt = DateTime.UtcNow,
                        CancelReason = activeSubscription.CancelReason
                    }
                };
            }
            catch (Exception ex)
            {
                return new CancelSubscriptionResponse
                {
                    Success = false,
                    Message = $"Error cancelling subscription: {ex.Message}"
                };
            }
        }
    }
}