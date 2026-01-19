using FastEndpoints;
using FDAAPI.App.FeatG65_FloodEventUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat65_FloodEventUpdate.DTOs;
using MediatR;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat65_FloodEventUpdate
{
    public class UpdateFloodEventEndpoint : Endpoint<UpdateFloodEventRequestDto, UpdateFloodEventResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateFloodEventEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/api/v1/admin/flood-events/{id}");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Update an existing flood event (Admin only)";
                s.Description = "Update flood event details by ID.";
            });
        }

        public override async Task HandleAsync(UpdateFloodEventRequestDto req, CancellationToken ct)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                               User.FindFirst("sub")?.Value ??
                               User.Identity?.Name;

            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                await SendAsync(new UpdateFloodEventResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify admin user",
                    StatusCode = 401
                }, 401, ct);
                return;
            }

            var idStr = Route<string>("id");
            if (!Guid.TryParse(idStr, out var id))
            {
                await SendAsync(new UpdateFloodEventResponseDto
                {
                    Success = false,
                    Message = "Invalid flood event ID format",
                    StatusCode = 400
                }, 400, ct);
                return;
            }

            var command = new UpdateFloodEventRequest(
                id,
                req.AdministrativeAreaId,
                req.StartTime,
                req.EndTime,
                req.PeakLevel,
                req.DurationHours,
                adminId
            );

            var result = await _mediator.Send(command, ct);

            await SendAsync(new UpdateFloodEventResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode
            }, (int)result.StatusCode, ct);
        }
    }
}

