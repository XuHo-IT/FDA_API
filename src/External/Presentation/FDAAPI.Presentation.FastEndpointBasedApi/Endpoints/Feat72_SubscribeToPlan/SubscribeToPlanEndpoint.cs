using FastEndpoints;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.FeatG72_SubscribeToPlan;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat72_SubscribeToPlan.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat72_SubscribeToPlan
{
    public class SubscribeToPlanEndpoint : Endpoint<SubscribeToPlanRequestDto, SubscribeToPlanResponseDto>
    {
        private readonly IMediator _mediator;

        public SubscribeToPlanEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/plan/subscription/subscribe");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Subscribe to a pricing plan";
                s.Description = "Subscribe user to FREE, PREMIUM, or MONITOR plan. Automatically cancels existing subscription.";
                s.ExampleRequest = new SubscribeToPlanRequestDto
                {
                    PlanCode = "PREMIUM",
                    DurationMonths = 12
                };
                s.ResponseExamples[200] = new SubscribeToPlanResponseDto
                {
                    Success = true,
                    Message = "Successfully subscribed to Premium Plan",
                    Subscription = new PlanSubscriptionDto
                    {
                        SubscriptionId = Guid.NewGuid(),
                        PlanCode = "PREMIUM",
                        PlanName = "Premium Plan",
                        Tier = "Premium",
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(1),
                        Status = "active"
                    }
                };
            });
            Tags("Subscription", "FE-13");
        }

        public override async Task HandleAsync(SubscribeToPlanRequestDto req, CancellationToken ct)
        {
            try
            {
                // Extract user ID from JWT
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    await SendAsync(new SubscribeToPlanResponseDto
                    {
                        Success = false,
                        Message = "Invalid user authentication"
                    }, 401, ct);
                    return;
                }

                // Send request to handler
                var command = new SubscribeToPlanRequest(
                    userId,
                    req.PlanCode,
                    req.DurationMonths
                );

                var result = await _mediator.Send(command, ct);

                // Map response
                var response = new SubscribeToPlanResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    Subscription = result.Subscription != null ? new PlanSubscriptionDto
                    {
                        SubscriptionId = result.Subscription.SubscriptionId,
                        PlanCode = result.Subscription.PlanCode,
                        PlanName = result.Subscription.PlanName,
                        Tier = result.Subscription.Tier,
                        StartDate = result.Subscription.StartDate,
                        EndDate = result.Subscription.EndDate,
                        Status = result.Subscription.Status
                    } : null
                };

                await SendAsync(response, result.Success ? 200 : 400, ct);
            }
            catch (Exception ex)
            {
                await SendAsync(new SubscribeToPlanResponseDto
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                }, 500, ct);
            }
        }
    }
}