using FastEndpoints;
using FDAAPI.App.FeatG40_GetAlertHistory;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat40_GetAlertHistory.DTOs;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat40_GetAlertHistory
{
    public class GetAlertHistoryEndpoint : Endpoint<GetAlertHistoryRequestDto, GetAlertHistoryResponseDto>
    {
        private readonly IMediator _mediator;

        public GetAlertHistoryEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/alerts/history");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Get alert history for current user";
                s.Description = "Retrieves paginated list of alerts that the user was notified about";
            });
            Tags("Alerts", "Notifications");
        }

        public override async Task HandleAsync(GetAlertHistoryRequestDto req, CancellationToken ct)
        {
            // Get authenticated user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new GetAlertHistoryResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify user"
                }, 401, ct);
                return;
            }

            var query = new GetAlertHistoryRequest(
                UserId: userId,
                StartDate: req.StartDate,
                EndDate: req.EndDate,
                Severity: req.Severity,
                Status: req.Status,
                PageNumber: req.PageNumber,
                PageSize: req.PageSize
            );

            var result = await _mediator.Send(query, ct);

            var statusCode = result.Success ? 200 : 500;

            await SendAsync(new GetAlertHistoryResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Alerts = result.Alerts,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            }, statusCode, ct);
        }
    }
}