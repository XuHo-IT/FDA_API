using FastEndpoints;
using FDAAPI.App.FeatG105_StationComponentCreate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG105_StationComponentCreate.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG105_StationComponentCreate
{
    public class CreateStationComponentEndpoint : Endpoint<CreateStationComponentRequestDto, StationComponentResponseDto>
    {
        private readonly IMediator _mediator;

        public CreateStationComponentEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/stations/{stationId}/components");
            Roles("ADMIN");
            Summary(s =>
            {
                s.Summary = "Create station component";
                s.Description = "Create a new hardware component for a station";
            });
        }

        public override async Task HandleAsync(CreateStationComponentRequestDto req, CancellationToken ct)
        {
            var stationId = Route<Guid>("stationId");

            var command = new CreateStationComponentRequest
            {
                StationId = stationId,
                ComponentType = req.ComponentType,
                Name = req.Name,
                Model = req.Model,
                SerialNumber = req.SerialNumber,
                FirmwareVersion = req.FirmwareVersion,
                Notes = req.Notes
            };

            var result = await _mediator.Send(command, ct);

            var response = new StationComponentResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Id = result.Id,
                Component = result.Component != null ? new StationComponentDto
                {
                    Id = result.Component.Id,
                    StationId = result.Component.StationId,
                    ComponentType = result.Component.ComponentType,
                    Name = result.Component.Name,
                    Model = result.Component.Model,
                    SerialNumber = result.Component.SerialNumber,
                    FirmwareVersion = result.Component.FirmwareVersion,
                    Status = result.Component.Status,
                    InstalledAt = result.Component.InstalledAt,
                    LastMaintenanceAt = result.Component.LastMaintenanceAt,
                    Notes = result.Component.Notes,
                    CreatedAt = result.Component.CreatedAt,
                    UpdatedAt = result.Component.UpdatedAt
                } : null
            };

            if (result.Success)
                await SendAsync(response, 201, ct);
            else
                await SendAsync(response, 400, ct);
        }
    }
}
