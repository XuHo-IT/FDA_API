using FDAAPI.App.FeatG68_DeleteSubscription;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat68_DeleteSubscription
{
    public class DeleteSubscriptionEndpoint : Endpoint<DeleteSubscriptionRequestDto, DeleteSubscriptionResponseDto>
    {
        private readonly IMediator _mediator;

        public DeleteSubscriptionEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/api/v1/alerts/subscriptions/{id}");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Policies("User");

            Summary(s =>
            {
                s.Summary = "Delete alert subscription";
                s.Description = "Delete an alert subscription by ID. Only the owner can delete their subscription.";
                s.Responses[200] = "Subscription deleted successfully";
                s.Responses[400] = "Bad request or permission denied";
                s.Responses[401] = "Unauthorized";
                s.Responses[404] = "Subscription not found";
            });

            Tags("Alerts");
        }

        public override async Task HandleAsync(DeleteSubscriptionRequestDto req, CancellationToken ct)
        {
            // Get UserId from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null)
            {
                await SendAsync(new DeleteSubscriptionResponseDto
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);

            // Send command to handler
            var command = new DeleteSubscriptionRequest(req.Id, userId);
            var result = await _mediator.Send(command, ct);

            var response = new DeleteSubscriptionResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            // Determine HTTP status code
            var statusCode = result.Success ? 200 :
                            result.Message.Contains("not found") ? 404 : 400;

            await SendAsync(response, statusCode, ct);
        }
    }
}