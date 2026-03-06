using FastEndpoints;
using FDAAPI.App.FeatG99_AlertTemplateList;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG99_AlertTemplateList.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG99_AlertTemplateList
{
    public class GetAlertTemplatesEndpoint : Endpoint<GetAlertTemplatesRequestDto, GetAlertTemplatesResponseDto>
    {
        private readonly IMediator _mediator;

        public GetAlertTemplatesEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/admin/alert-templates");
            Roles("ADMIN");
            Summary(s =>
            {
                s.Summary = "Get alert templates";
                s.Description = "List all alert notification templates with optional filters";
            });
        }

        public override async Task HandleAsync(GetAlertTemplatesRequestDto req, CancellationToken ct)
        {
            var command = new GetAlertTemplatesRequest(req.IsActive, req.Channel, req.Severity);
            var result = await _mediator.Send(command, ct);

            var templates = result.Templates.Select(t => new AlertTemplateResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Channel = t.Channel,
                Severity = t.Severity,
                TitleTemplate = t.TitleTemplate,
                BodyTemplate = t.BodyTemplate,
                IsActive = t.IsActive,
                SortOrder = t.SortOrder,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });

            await SendOkAsync(new GetAlertTemplatesResponseDto
            {
                Success = true,
                Templates = templates.ToList()
            }, ct);
        }
    }
}
