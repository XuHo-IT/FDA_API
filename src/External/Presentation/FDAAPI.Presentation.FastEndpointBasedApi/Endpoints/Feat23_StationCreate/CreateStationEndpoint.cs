using FastEndpoints;
using FDAAPI.App.FeatG23_StationCreate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat21_StationCreate.DTOs;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat21_StationCreate
{
    public class CreateStationEndpoint : Endpoint<CreateStationRequestDto, CreateStationReponseDto>
    {
        private readonly IMediator _mediator;
        public CreateStationEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/create-station");
            Policies("Admin"); 
            AllowAnonymous();
            Summary(s => {
                s.Summary = "Create a new monitoring station.";
                s.Description = "Receive data from DTO and send it to MediatR Handler.";
            });
        }

        public override async Task HandleAsync(CreateStationRequestDto req, CancellationToken ct)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                               User.FindFirst("sub")?.Value ??
                               User.Identity?.Name;

            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                await SendAsync(new CreateStationReponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify admin user"
                }, 401, ct);
                return;
            }

            var command = new CreateStationRequest(
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
              req.InstalledAt,
              adminId 
                 );

            var result = await _mediator.Send(command, ct);

            await SendAsync(new CreateStationReponseDto
            {
                Success = result.Success,
                Message = result.Message,
                StatusCode = (int)result.StatusCode,
                Data = result.Data
            }, 201, ct);
        }
    }
}
