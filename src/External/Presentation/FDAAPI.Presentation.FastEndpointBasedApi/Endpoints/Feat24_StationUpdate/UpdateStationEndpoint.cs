using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG24_StationUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat22_StationUpdate.DTOs;
using MediatR;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat22_StationUpdate
{
    public class UpdateStationEndpoint : Endpoint<UpdateStationRequestDto, UpdateStationResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateStationEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/api/v1/stations/station/{id}");
            Policies("Admin");
            Summary(s => {
                s.Summary = "Update an existing monitoring station.";
                s.Description = "Update station details by ID.";
            });
        }

        public override async Task HandleAsync(UpdateStationRequestDto req, CancellationToken ct)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                               User.FindFirst("sub")?.Value ??
                               User.Identity?.Name;

            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                await SendAsync(new UpdateStationResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify admin user",
                    StatusCode = 401
                }, 401, ct);
                return;
            }

            var command = new UpdateStationRequest(
                req.Id,
                req.Code,
                req.Name,
                req.LocationDesc,
                req.Latitude,
                req.Longitude,
                req.RoadName,
                req.Direction,
                req.Status,
                req.ThresholdWarning,
                req.ThresholdCritical,
                req.AdministrativeAreaId,
                req.InstalledAt,
                req.LastSeenAt,
                adminId
            );

            var result = await _mediator.Send(command, ct);

            await SendAsync(new UpdateStationResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode
            }, (int)result.StatusCode, ct);
        }
    }
}
