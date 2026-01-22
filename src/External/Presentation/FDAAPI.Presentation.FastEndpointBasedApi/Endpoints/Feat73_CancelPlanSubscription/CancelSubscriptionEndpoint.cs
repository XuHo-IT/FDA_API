using FastEndpoints;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.FeatG73_CancelSubscription;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat73_CancelPlanSubscription.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat73_CancelSubscription
{
    public class CancelSubscriptionEndpoint : Endpoint<CancelSubscriptionRequestDto, CancelSubscriptionResponseDto>
    {
        private readonly IMediator _mediator;

        public CancelSubscriptionEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Delete("/api/v1/plan/subscription/cancel");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            AllowAnonymous();
            //Policies("User", "Admin", "Authority");
            Summary(s =>
            {
                s.Summary = "Cancel current subscription";
                s.Description = "Cancel active subscription and return to Free tier. Cannot cancel if already on Free plan.";
                s.ExampleRequest = new CancelSubscriptionRequestDto
                {
                    CancelReason = "Too expensive"
                };
            });
            Tags("Subscription", "FE-13");
        }

        public override async Task HandleAsync(CancelSubscriptionRequestDto req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new CancelSubscriptionResponseDto
                {
                    Success = false,
                    Message = "Invalid user authentication"
                }, 401, ct);
                return;
            }

            var command = new CancelSubscriptionRequest(userId, req.CancelReason);
            var result = await _mediator.Send(command, ct);

            var response = new CancelSubscriptionResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                CancelledSubscription = result.CancelledSubscription != null ? new CancelledPlanSubscriptionDto
                {
                    SubscriptionId = result.CancelledSubscription.SubscriptionId,
                    PlanName = result.CancelledSubscription.PlanName,
                    PreviousTier = result.CancelledSubscription.PreviousTier,
                    CancelledAt = result.CancelledSubscription.CancelledAt,
                    CancelReason = result.CancelledSubscription.CancelReason
                } : null
            };

            await SendAsync(response, result.Success ? 200 : 400, ct);
        }
    }
}