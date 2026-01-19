using FastEndpoints;
using FDAAPI.App.FeatG66_FloodEventDelete;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat66_FloodEventDelete.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat66_FloodEventDelete
{
    public class DeleteFloodEventEndpoint : EndpointWithoutRequest<DeleteFloodEventResponseDto>
    {
        private readonly IMediator _mediator;

        public DeleteFloodEventEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/api/v1/admin/flood-events/{id}");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Delete a flood event (Admin only)";
                s.Description = "Remove a flood event from the system.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (!Guid.TryParse(idStr, out var id))
            {
                await SendAsync(new DeleteFloodEventResponseDto
                {
                    Success = false,
                    Message = "Invalid flood event ID format",
                    StatusCode = 400
                }, 400, ct);
                return;
            }

            var command = new DeleteFloodEventRequest(id);
            var result = await _mediator.Send(command, ct);

            await SendAsync(new DeleteFloodEventResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode
            }, (int)result.StatusCode, ct);
        }
    }
}

