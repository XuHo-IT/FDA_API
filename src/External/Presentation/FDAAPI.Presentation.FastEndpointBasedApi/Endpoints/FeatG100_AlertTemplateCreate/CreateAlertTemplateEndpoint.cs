using FastEndpoints;
using FDAAPI.App.FeatG100_AlertTemplateCreate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG100_AlertTemplateCreate.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG100_AlertTemplateCreate
{
    public class CreateAlertTemplateEndpoint : Endpoint<CreateAlertTemplateRequestDto, CreateAlertTemplateResponseDto>
    {
        private readonly IMediator _mediator;

        public CreateAlertTemplateEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/admin/alert-templates");
            Roles("ADMIN");
            Summary(s =>
            {
                s.Summary = "Create alert template";
                s.Description = "Create a new alert notification template";
            });
        }

        public override async Task HandleAsync(CreateAlertTemplateRequestDto req, CancellationToken ct)
        {
            var userId = GetUserId();

            var command = new CreateAlertTemplateRequest(
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

            var response = new CreateAlertTemplateResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Id = result.Id
            };

            if (result.Success)
                await SendAsync(response, 201, ct);
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
