using FastEndpoints;
using FDAAPI.App.FeatG103_AlertTemplateGet;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG103_AlertTemplateGet.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG103_AlertTemplateGet
{
    public class GetAlertTemplateByIdEndpoint : Endpoint<EmptyRequest, GetAlertTemplateByIdResponseDto>
    {
        private readonly IMediator _mediator;

        public GetAlertTemplateByIdEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/admin/alert-templates/{id}");
            Roles("ADMIN");
            Summary(s =>
            {
                s.Summary = "Get alert template by ID";
                s.Description = "Get a single alert template by its ID";
            });
        }

        public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
        {
            var templateId = Route<Guid>("id");

            var command = new GetAlertTemplateByIdRequest(templateId);
            var result = await _mediator.Send(command, ct);

            if (!result.Success || result.Template == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var t = result.Template;
            await SendOkAsync(new GetAlertTemplateByIdResponseDto
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
            }, ct);
        }
    }
}
