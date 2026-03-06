using FastEndpoints;
using FDAAPI.App.FeatG85_FloodReportDelete;
using MediatR;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat85_FloodReportDelete
{
    public class DeleteFloodReportEndpoint : Endpoint<EmptyRequest, DeleteFloodReportResponse>
    {
        private readonly IMediator _mediator;

        public DeleteFloodReportEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/api/v1/flood-reports/{id}");
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Delete a flood report";
                s.Description = "Delete a flood report and all associated media files";
            });
            Description(b => b
                .Produces(200)
                .Produces(401)
                .Produces(404));
            Tags("FloodReports");
        }

        public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
        {
            // Extract report ID from route
            var reportId = Route<Guid>("id");

            // Get user info from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                await SendAsync(new DeleteFloodReportResponse
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "USER";

            // Map to handler request
            var command = new DeleteFloodReportRequest(
                reportId,
                userId,
                userRole
            );

            // Send to handler
            var result = await _mediator.Send(command, ct);

            // Return appropriate status code
            var statusCode = result.Success ? 200 : (reportId == Guid.Empty ? 400 : 404);
            await SendAsync(result, statusCode, ct);
        }
    }
}
