using FastEndpoints;
using FDAAPI.App.FeatG109_StationComponentGet;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG109_StationComponentGet.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG109_StationComponentGet
{
    public class GetStationComponentByIdEndpoint : Endpoint<EmptyRequest, StationComponentResponseDto>
    {
        private readonly IMediator _mediator;

        public GetStationComponentByIdEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/stations/{stationId}/components/{id}");
            Roles("ADMIN", "USER");
            Summary(s =>
            {
                s.Summary = "Get station component by ID";
                s.Description = "Get a specific hardware component by ID";
            });
        }

        public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
        {
            var id = Route<Guid>("id");

            var command = new GetStationComponentByIdRequest { Id = id };
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
                await SendAsync(response, 404, ct);
        }
    }
}
