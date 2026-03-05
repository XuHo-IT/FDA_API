using FastEndpoints;
using FDAAPI.App.FeatG62_FloodEventCreate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat62_FloodEventCreate.DTOs;
using MediatR;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat62_FloodEventCreate
{
    public class CreateFloodEventEndpoint : Endpoint<CreateFloodEventRequestDto, CreateFloodEventResponseDto>
    {
        private readonly IMediator _mediator;

        public CreateFloodEventEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/admin/flood-events");
            Policies("Admin");
            Summary(s =>
            {
                s.Summary = "Create a new flood event (Admin only)";
                s.Description = "Creates a new flood event record for an administrative area.";
            });
        }

        public override async Task HandleAsync(CreateFloodEventRequestDto req, CancellationToken ct)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                               User.FindFirst("sub")?.Value ??
                               User.Identity?.Name;

            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                await SendAsync(new CreateFloodEventResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify admin user"
                }, 401, ct);
                return;
            }

            var command = new CreateFloodEventRequest(
                req.AdministrativeAreaId,
                req.StartTime,
                req.EndTime,
                req.PeakLevel,
                req.DurationHours,
                adminId
            );

            var result = await _mediator.Send(command, ct);

            await SendAsync(new CreateFloodEventResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data
            }, (int)result.StatusCode, ct);
        }
    }
}

