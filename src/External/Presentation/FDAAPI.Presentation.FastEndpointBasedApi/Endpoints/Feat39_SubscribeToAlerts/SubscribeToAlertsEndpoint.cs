using FastEndpoints;
using FDAAPI.App.FeatG39_SubscribeToAlerts;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat39_SubscribeToAlerts.DTOs;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat39_SubscribeToAlerts
{
    public class SubscribeToAlertsEndpoint : Endpoint<SubscribeToAlertsRequestDto, SubscribeToAlertsResponseDto>
    {
        private readonly IMediator _mediator;

        public SubscribeToAlertsEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/alerts/subscriptions");
            Policies("User", "Admin", "Authority");
            Summary(s =>
            {
                s.Summary = "Subscribe to alerts for a station or area";
                s.Description = "User subscribes to receive notifications when alerts are triggered";
            });
            Tags("Alerts", "Notifications");
        }

        public override async Task HandleAsync(SubscribeToAlertsRequestDto req, CancellationToken ct)
        {
            // Get authenticated user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new SubscribeToAlertsResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify user"
                }, 401, ct);
                return;
            }

            var command = new SubscribeToAlertsRequest(
                UserId: userId,
                AreaId: req.AreaId,
                StationId: req.StationId,
                MinSeverity: req.MinSeverity,
                EnablePush: req.EnablePush,
                EnableEmail: req.EnableEmail,
                EnableSms: req.EnableSms,
                QuietHoursStart: req.QuietHoursStart,
                QuietHoursEnd: req.QuietHoursEnd
            );

            var result = await _mediator.Send(command, ct);

            var statusCode = result.Success ? 201 : 400;

            await SendAsync(new SubscribeToAlertsResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                SubscriptionId = result.SubscriptionId
            }, statusCode, ct);
        }
    }
}