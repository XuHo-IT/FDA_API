using FastEndpoints;
using FDAAPI.App.FeatG41_UpdateAlertPreferences;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat41_UpdateAlertPreferences.DTOs;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat41_UpdateAlertPreferences
{
    public class UpdateAlertPreferencesEndpoint : Endpoint<UpdateAlertPreferencesRequestDto, UpdateAlertPreferencesResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateAlertPreferencesEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/api/v1/alerts/subscriptions/{subscriptionId}");
            Policies("User", "Admin", "Authority");
            Summary(s =>
            {
                s.Summary = "Update alert subscription preferences";
                s.Description = "Update notification channels, severity threshold, quiet hours for an existing subscription";
            });
            Tags("Alerts", "Notifications");
        }

        public override async Task HandleAsync(UpdateAlertPreferencesRequestDto req, CancellationToken ct)
        {
            // Get authenticated user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new UpdateAlertPreferencesResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify user"
                }, 401, ct);
                return;
            }

            var command = new UpdateAlertPreferencesRequest(
                SubscriptionId: req.SubscriptionId,
                UserId: userId,
                MinSeverity: req.MinSeverity,
                EnablePush: req.EnablePush,
                EnableEmail: req.EnableEmail,
                EnableSms: req.EnableSms,
                QuietHoursStart: req.QuietHoursStart,
                QuietHoursEnd: req.QuietHoursEnd
            );

            var result = await _mediator.Send(command, ct);

            var statusCode = result.Success ? 200 : 400;

            await SendAsync(new UpdateAlertPreferencesResponseDto
            {
                Success = result.Success,
                Message = result.Message
            }, statusCode, ct);
        }
    }
}