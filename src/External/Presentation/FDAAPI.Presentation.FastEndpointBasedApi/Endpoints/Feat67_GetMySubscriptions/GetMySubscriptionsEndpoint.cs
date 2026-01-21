using FDAAPI.App.FeatG67_GetMySubscriptions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat67_GetMySubscriptions
{
    public class GetMySubscriptionsEndpoint : EndpointWithoutRequest<GetMySubscriptionsResponseDto>
    {
        private readonly IMediator _mediator;

        public GetMySubscriptionsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/alerts/subscriptions/me");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Policies("User");

            Summary(s =>
            {
                s.Summary = "Get my alert subscriptions";
                s.Description = "Get all alert subscriptions for the current user";
            });

            Tags("Alerts");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            // Get UserId from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null)
            {
                await SendAsync(new GetMySubscriptionsResponseDto
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var query = new GetMySubscriptionsRequest(userId);
            var result = await _mediator.Send(query, ct);

            var response = new GetMySubscriptionsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Data = new DTOs.SubscriptionListDto
                {
                    Subscriptions = result.Subscriptions,
                    TotalCount = result.TotalCount
                }
            };

            await SendAsync(response, result.Success ? 200 : 500, ct);
        }
    }
}