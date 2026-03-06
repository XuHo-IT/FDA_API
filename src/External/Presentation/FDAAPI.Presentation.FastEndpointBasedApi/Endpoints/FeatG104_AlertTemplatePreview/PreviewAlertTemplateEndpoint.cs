using FastEndpoints;
using FDAAPI.App.FeatG104_AlertTemplatePreview;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG104_AlertTemplatePreview.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG104_AlertTemplatePreview
{
    public class PreviewAlertTemplateEndpoint : Endpoint<PreviewAlertTemplateRequestDto, PreviewAlertTemplateResponseDto>
    {
        private readonly IMediator _mediator;

        public PreviewAlertTemplateEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/admin/alert-templates/preview");
            Roles("ADMIN");
            Summary(s =>
            {
                s.Summary = "Preview alert template";
                s.Description = "Preview how a template will render with sample data";
            });
        }

        public override async Task HandleAsync(PreviewAlertTemplateRequestDto req, CancellationToken ct)
        {
            var command = new PreviewAlertTemplateRequest(
                req.TemplateId,
                req.TitleTemplate,
                req.BodyTemplate,
                req.StationName,
                req.WaterLevel,
                req.Threshold,
                req.Severity,
                req.Address,
                req.Message
            );

            var result = await _mediator.Send(command, ct);

            var response = new PreviewAlertTemplateResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Title = result.Title,
                Body = result.Body
            };

            if (result.Success)
                await SendOkAsync(response, ct);
            else
                await SendAsync(response, 400, ct);
        }
    }
}
