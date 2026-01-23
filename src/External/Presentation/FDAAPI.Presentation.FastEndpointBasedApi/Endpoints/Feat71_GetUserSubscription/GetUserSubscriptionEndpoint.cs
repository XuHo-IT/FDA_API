using FastEndpoints;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.FeatG71_GetUserSubscription;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat71_GetUserSubscription.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat71_GetUserSubscription
{
    public class GetUserSubscriptionEndpoint : EndpointWithoutRequest<GetUserSubscriptionResponseDto>
    {
        private readonly IMediator _mediator;

        public GetUserSubscriptionEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/plan/subscription/current");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Get current user's subscription tier and plan";
                s.Description = "Retrieves the authenticated user's active subscription including tier, available channels, dispatch delays, and pricing information";
                s.ResponseExamples[200] = new GetUserSubscriptionResponseDto
                {
                    Success = true,
                    Message = "Subscription retrieved successfully",
                    Subscription = new UserPlanSubscriptionDto
                    {
                        Tier = "Premium",
                        TierCode = "PREMIUM",
                        PlanName = "Premium Plan",
                        Description = "Priority alerts with SMS, Email, and Push - faster delivery",
                        PriceMonth = 9.99m,
                        PriceYear = 99.99m,
                        StartDate = DateTime.UtcNow.AddMonths(-1),
                        EndDate = DateTime.UtcNow.AddMonths(11),
                        Status = "active",
                        AvailableChannels = new List<string> { "Push", "Email", "SMS", "InApp" },
                        DispatchDelay = new DispatchDelayDto
                        {
                            HighPrioritySeconds = 0,
                            LowPrioritySeconds = 20
                        },
                        MaxRetries = 3
                    }
                };
            });
            Tags("Subscription", "User Management", "FE-13");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                // Extract user ID from JWT token
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    await SendAsync(new GetUserSubscriptionResponseDto
                    {
                        Success = false,
                        Message = "Invalid user authentication"
                    }, 401, ct);
                    return;
                }

                // Send request to handler
                var request = new GetUserSubscriptionRequest(userId);
                var result = await _mediator.Send(request, ct);

                // Map response
                var response = new GetUserSubscriptionResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    Subscription = result.Subscription != null ? new UserPlanSubscriptionDto
                    {
                        Tier = result.Subscription.Tier,
                        TierCode = result.Subscription.TierCode,
                        PlanName = result.Subscription.PlanName,
                        Description = result.Subscription.Description,
                        PriceMonth = result.Subscription.PriceMonth,
                        PriceYear = result.Subscription.PriceYear,
                        StartDate = result.Subscription.StartDate,
                        EndDate = result.Subscription.EndDate,
                        Status = result.Subscription.Status,
                        AvailableChannels = result.Subscription.AvailableChannels,
                        DispatchDelay = new DispatchDelayDto
                        {
                            HighPrioritySeconds = result.Subscription.DispatchDelay.HighPrioritySeconds,
                            LowPrioritySeconds = result.Subscription.DispatchDelay.LowPrioritySeconds
                        },
                        MaxRetries = result.Subscription.MaxRetries
                    } : null
                };

                await SendAsync(response, result.Success ? 200 : 500, ct);
            }
            catch (Exception ex)
            {
                await SendAsync(new GetUserSubscriptionResponseDto
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                }, 500, ct);
            }
        }
    }
}