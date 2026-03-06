using FastEndpoints;
using FDAAPI.App.FeatG108_StationComponentList;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG108_StationComponentList.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG108_StationComponentList
{
    public class GetStationComponentsEndpoint : Endpoint<EmptyRequest, StationComponentListResponseDto>
    {
        private readonly IMediator _mediator;

        public GetStationComponentsEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/stations/{stationId}/components");
            Roles("ADMIN", "USER");
            Summary(s =>
            {
                s.Summary = "Get station components";
                s.Description = "Get all hardware components for a station";
            });
        }

        public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
        {
            var stationId = Route<Guid>("stationId");

            var command = new GetStationComponentsRequest { StationId = stationId };
            var result = await _mediator.Send(command, ct);

            var response = new StationComponentListResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Components = result.Components?.Select(c => new StationComponentDto
                {
                    Id = c.Id,
                    StationId = c.StationId,
                    ComponentType = c.ComponentType,
                    Name = c.Name,
                    Model = c.Model,
                    SerialNumber = c.SerialNumber,
                    FirmwareVersion = c.FirmwareVersion,
                    Status = c.Status,
                    InstalledAt = c.InstalledAt,
                    LastMaintenanceAt = c.LastMaintenanceAt,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList() ?? new List<StationComponentDto>()
            };

            await SendAsync(response, 200, ct);
        }
    }
}
