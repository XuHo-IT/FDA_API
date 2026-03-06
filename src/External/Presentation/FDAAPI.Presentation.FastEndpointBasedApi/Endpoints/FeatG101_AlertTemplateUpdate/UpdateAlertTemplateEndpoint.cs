using FastEndpoints;
using FDAAPI.App.FeatG101_AlertTemplateUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG101_AlertTemplateUpdate.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG101_AlertTemplateUpdate
{
    public class UpdateAlertTemplateEndpoint : Endpoint<UpdateAlertTemplateRequestDto, UpdateAlertTemplateResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateAlertTemplateEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Put("/api/v1/admin/alert-templates/{id}");
            Roles("ADMIN");
            Summary(s =>
            {
                s.Summary = "Update alert template";
                s.Description = "Update an existing alert notification template";
            });
        }

        public override async Task HandleAsync(UpdateAlertTemplateRequestDto req, CancellationToken ct)
        {
            var userId = GetUserId();
            var templateId = Route<Guid>("id");

            var command = new UpdateAlertTemplateRequest(
                templateId,
                req.Name,
                req.Channel,
                req.Severity,
                req.TitleTemplate,
                req.BodyTemplate,
                req.IsActive,
                req.SortOrder,
                userId
            );

            var result = await _mediator.Send(command, ct);

            var response = new UpdateAlertTemplateResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Id = templateId
            };

            if (result.Success)
                await SendAsync(response, 200, ct);
            else
                await SendAsync(response, 400, ct);
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
}
