using FastEndpoints;
using FDAAPI.App.FeatG107_StationComponentDelete;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG107_StationComponentDelete.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG107_StationComponentDelete
{
    public class DeleteStationComponentEndpoint : Endpoint<EmptyRequest, StationComponentResponseDto>
    {
        private readonly IMediator _mediator;

        public DeleteStationComponentEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Delete("/api/v1/stations/{stationId}/components/{id}");
            Roles("ADMIN");
            Summary(s =>
            {
                s.Summary = "Delete station component";
                s.Description = "Delete a hardware component";
            });
        }

        public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
        {
            var id = Route<Guid>("id");

            var command = new DeleteStationComponentRequest { Id = id };
            var result = await _mediator.Send(command, ct);

            var response = new StationComponentResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Id = result.Id
            };

            if (result.Success)
                await SendAsync(response, 200, ct);
            else
                await SendAsync(response, 404, ct);
        }
    }
}
