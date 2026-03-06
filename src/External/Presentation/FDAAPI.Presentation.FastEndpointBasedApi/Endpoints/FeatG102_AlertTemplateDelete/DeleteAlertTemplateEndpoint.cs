using FastEndpoints;
using FDAAPI.App.FeatG102_AlertTemplateDelete;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG102_AlertTemplateDelete.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG102_AlertTemplateDelete
{
    public class DeleteAlertTemplateEndpoint : Endpoint<EmptyRequest, DeleteAlertTemplateResponseDto>
    {
        private readonly IMediator _mediator;

        public DeleteAlertTemplateEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Delete("/api/v1/admin/alert-templates/{id}");
            Roles("ADMIN");
            Summary(s =>
            {
                s.Summary = "Delete alert template";
                s.Description = "Delete an alert notification template";
            });
        }

        public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
        {
            var templateId = Route<Guid>("id");

            var command = new DeleteAlertTemplateRequest(templateId);
            var result = await _mediator.Send(command, ct);

            var response = new DeleteAlertTemplateResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            if (result.Success)
                await SendAsync(response, 200, ct);
            else
                await SendAsync(response, 404, ct);
        }
    }
}
