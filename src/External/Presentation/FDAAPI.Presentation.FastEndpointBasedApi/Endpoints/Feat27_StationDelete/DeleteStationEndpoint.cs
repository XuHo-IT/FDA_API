using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG27_StationDelete;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat24_StationDelete.DTOs;
using MediatR;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat24_StationDelete
{
    public class DeleteStationEndpoint : EndpointWithoutRequest<DeleteStationResponseDto>
    {
        private readonly IMediator _mediator;

        public DeleteStationEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/api/v1/stations/station/{id}");
            Policies("Admin");
            Summary(s => {
                s.Summary = "Delete a monitoring station.";
                s.Description = "Remove a monitoring station from the system.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                               User.FindFirst("sub")?.Value ??
                               User.Identity?.Name;

            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                await SendAsync(new DeleteStationResponseDto
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
                await SendAsync(new DeleteStationResponseDto
                {
                    Success = false,
                    Message = "Invalid station ID format",
                    StatusCode = 400
                }, 400, ct);
                return;
            }

            var command = new DeleteStationRequest(id);
            var result = await _mediator.Send(command, ct);

            await SendAsync(new DeleteStationResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode
            }, (int)result.StatusCode, ct);
        }
    }
}
