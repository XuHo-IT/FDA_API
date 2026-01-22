using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG71_GetUserSubscription
{
    /// <summary>
    /// Handler to retrieve user's active subscription with plan details
    /// </summary>
    public class GetUserSubscriptionHandler : IRequestHandler<GetUserSubscriptionRequest, GetUserSubscriptionResponse>
    {
        private readonly IUserSubscriptionRepository _subscriptionRepo;

        public GetUserSubscriptionHandler(IUserSubscriptionRepository subscriptionRepo)
        {
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<GetUserSubscriptionResponse> Handle(
            GetUserSubscriptionRequest request,
            CancellationToken ct)
        {
            try
            {
                // Get active subscription with plan details
                var subscription = await _subscriptionRepo.GetActiveSubscriptionAsync(request.UserId, ct);

                // Default to Free tier if no active subscription
                if (subscription == null || subscription.Plan == null)
                {
                    return new GetUserSubscriptionResponse
                    {
                        Success = true,
                        Message = "User is on Free tier (no active subscription)",
                        Subscription = new UserPlanSubscriptionDto
                        {
                            Tier = "Free",
                            TierCode = "FREE",
                            PlanName = "Free Plan",
                            Description = "Basic flood alerts with push and email notifications",
                            PriceMonth = 0,
                            PriceYear = 0,
                            Status = "free",
                            AvailableChannels = new List<string> { "Push", "Email" },
                            DispatchDelay = new DispatchDelayDto
                            {
                                HighPrioritySeconds = 60,
                                LowPrioritySeconds = 120
                            },
                            MaxRetries = 1
                        }
                    };
                }

                // Map subscription to DTO
                var tier = subscription.Plan.Tier;
                var availableChannels = GetAvailableChannels(tier);
                var dispatchDelay = GetDispatchDelay(tier);
                var maxRetries = GetMaxRetries(tier);

                return new GetUserSubscriptionResponse
                {
                    Success = true,
                    Message = "Subscription retrieved successfully",
                    Subscription = new UserPlanSubscriptionDto
                    {
                        Tier = tier.ToString(),
                        TierCode = subscription.Plan.Code,
                        PlanName = subscription.Plan.Name,
                        Description = subscription.Plan.Description,
                        PriceMonth = subscription.Plan.PriceMonth,
                        PriceYear = subscription.Plan.PriceYear,
                        StartDate = subscription.StartDate,
                        EndDate = subscription.EndDate,
                        Status = subscription.Status,
                        AvailableChannels = availableChannels,
                        DispatchDelay = dispatchDelay,
                        MaxRetries = maxRetries
                    }
                };
            }
            catch (Exception ex)
            {
                return new GetUserSubscriptionResponse
                {
                    Success = false,
                    Message = $"Error retrieving subscription: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get available notification channels based on tier
        /// </summary>
        private List<string> GetAvailableChannels(SubscriptionTier tier)
        {
            return tier switch
            {
                SubscriptionTier.Monitor => new List<string> { "Push", "Email", "SMS", "InApp" },
                SubscriptionTier.Premium => new List<string> { "Push", "Email", "SMS", "InApp" },
                SubscriptionTier.Free => new List<string> { "Push", "Email", "InApp" },
                _ => new List<string> { "Push", "Email", "InApp" }
            };
        }

        /// <summary>
        /// Get dispatch delay based on tier (aligned with PriorityRoutingService)
        /// </summary>
        private DispatchDelayDto GetDispatchDelay(SubscriptionTier tier)
        {
            return tier switch
            {
                SubscriptionTier.Monitor => new DispatchDelayDto
                {
                    HighPrioritySeconds = 0,    // Immediate
                    LowPrioritySeconds = 10
                },
                SubscriptionTier.Premium => new DispatchDelayDto
                {
                    HighPrioritySeconds = 0,    // Immediate
                    LowPrioritySeconds = 20
                },
                SubscriptionTier.Free => new DispatchDelayDto
                {
                    HighPrioritySeconds = 60,
                    LowPrioritySeconds = 120
                },
                _ => new DispatchDelayDto
                {
                    HighPrioritySeconds = 60,
                    LowPrioritySeconds = 120
                }
            };
        }

        /// <summary>
        /// Get max retry attempts based on tier
        /// </summary>
        private int GetMaxRetries(SubscriptionTier tier)
        {
            return tier switch
            {
                SubscriptionTier.Monitor => 5,
                SubscriptionTier.Premium => 3,
                SubscriptionTier.Free => 1,
                _ => 1
            };
        }
    }
}