using FastEndpoints;
using FDAAPI.App.FeatG106_StationComponentUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG106_StationComponentUpdate.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG106_StationComponentUpdate
{
    public class UpdateStationComponentEndpoint : Endpoint<UpdateStationComponentRequestDto, StationComponentResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateStationComponentEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Put("/api/v1/stations/{stationId}/components/{id}");
            Roles("ADMIN");
            Summary(s =>
            {
                s.Summary = "Update station component";
                s.Description = "Update an existing hardware component";
            });
        }

        public override async Task HandleAsync(UpdateStationComponentRequestDto req, CancellationToken ct)
        {
            var id = Route<Guid>("id");

            var command = new UpdateStationComponentRequest
            {
                Id = id,
                Name = req.Name,
                Model = req.Model,
                SerialNumber = req.SerialNumber,
                FirmwareVersion = req.FirmwareVersion,
                Status = req.Status,
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
                await SendAsync(response, 200, ct);
            else
                await SendAsync(response, 400, ct);
        }
    }
}
