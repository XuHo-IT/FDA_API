using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG72_SubscribeToPlan
{
    /// <summary>
    /// Handler to subscribe user to a pricing plan
    /// </summary>
    public class SubscribeToPlanHandler : IRequestHandler<SubscribeToPlanRequest, SubscribeToPlanResponse>
    {
        private readonly IPricingPlanRepository _planRepo;
        private readonly IUserSubscriptionRepository _subscriptionRepo;

        public SubscribeToPlanHandler(
            IPricingPlanRepository planRepo,
            IUserSubscriptionRepository subscriptionRepo)
        {
            _planRepo = planRepo;
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<SubscribeToPlanResponse> Handle(
            SubscribeToPlanRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Validate plan exists
                var plan = await _planRepo.GetByCodeAsync(request.PlanCode.ToUpper(), ct);
                if (plan == null)
                {
                    return new SubscribeToPlanResponse
                    {
                        Success = false,
                        Message = $"Plan '{request.PlanCode}' not found or inactive"
                    };
                }

                // 2. Check existing active subscription
                var existingSubscription = await _subscriptionRepo.GetActiveSubscriptionAsync(request.UserId, ct);

                // 3. If exists, cancel it (set EndDate to now)
                if (existingSubscription != null)
                {
                    existingSubscription.Status = "cancelled";
                    existingSubscription.EndDate = DateTime.UtcNow;
                    existingSubscription.CancelReason = $"Switched to {plan.Name}";
                    existingSubscription.UpdatedBy = request.UserId;
                    existingSubscription.UpdatedAt = DateTime.UtcNow;
                    await _subscriptionRepo.UpdateAsync(existingSubscription, ct);
                }

                // 4. Create new subscription
                var startDate = DateTime.UtcNow;
                var endDate = request.PlanCode.ToUpper() == "FREE"
                    ? DateTime.UtcNow.AddYears(100) // Free never expires
                    : startDate.AddMonths(request.DurationMonths);

                var subscription = new UserSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    PlanId = plan.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = "active",
                    RenewMode = "manual",
                    CreatedBy = request.UserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = request.UserId,
                    UpdatedAt = DateTime.UtcNow
                };

                await _subscriptionRepo.CreateAsync(subscription, ct);

                return new SubscribeToPlanResponse
                {
                    Success = true,
                    Message = $"Successfully subscribed to {plan.Name}",
                    Subscription = new PlanSubscriptionDto
                    {
                        SubscriptionId = subscription.Id,
                        PlanCode = plan.Code,
                        PlanName = plan.Name,
                        Tier = plan.Tier.ToString(),
                        StartDate = subscription.StartDate,
                        EndDate = subscription.EndDate,
                        Status = subscription.Status
                    }
                };
            }
            catch (Exception ex)
            {
                return new SubscribeToPlanResponse
                {
                    Success = false,
                    Message = $"Error subscribing to plan: {ex.Message}"
                };
            }
        }
    }
}