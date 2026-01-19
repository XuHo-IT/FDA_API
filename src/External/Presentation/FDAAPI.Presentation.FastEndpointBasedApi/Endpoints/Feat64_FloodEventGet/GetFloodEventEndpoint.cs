using FastEndpoints;
using FDAAPI.App.FeatG64_FloodEventGet;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat64_FloodEventGet.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat64_FloodEventGet
{
    public class GetFloodEventEndpoint : EndpointWithoutRequest<GetFloodEventResponseDto>
    {
        private readonly IMediator _mediator;

        public GetFloodEventEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/admin/flood-events/{id}");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Get flood event details (Admin only)";
                s.Description = "Retrieve flood event information by its unique identifier.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (!Guid.TryParse(idStr, out var id))
            {
                await SendAsync(new GetFloodEventResponseDto
                {
                    Success = false,
                    Message = "Invalid flood event ID format",
                    StatusCode = 400
                }, 400, ct);
                return;
            }

            var request = new GetFloodEventRequest(id);
            var result = await _mediator.Send(request, ct);

            await SendAsync(new GetFloodEventResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                FloodEvent = result.FloodEvent
            }, (int)result.StatusCode, ct);
        }
    }
}

