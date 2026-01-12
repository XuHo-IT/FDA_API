using FastEndpoints;
using FDAAPI.App.FeatG1_SensorReadingCreate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat1_SensorReadingCreate.DTOs;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat1_SensorReadingCreate
{
    public class CreateSensorReadingEndpoint : Endpoint<CreateSensorReadingRequestDto, CreateSensorReadingResponseDto>
    {
        private readonly IMediator _mediator;

        public CreateSensorReadingEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/sensor-readings");
            Policies("Admin", "Authority");
            Summary(s => {
                s.Summary = "Create a new sensor reading";
                s.Description = "Records water level measurement from IoT sensor";
            });
            Tags("SensorReadings", "IoT");
        }

        public override async Task HandleAsync(CreateSensorReadingRequestDto req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new CreateSensorReadingResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify user"
                }, 401, ct);
                return;
            }

            var command = new CreateSensorReadingRequest(
                req.StationId,
                req.Value,
                req.Distance,
                req.SensorHeight,
                req.Unit ?? "cm",
                req.Status,
                req.MeasuredAt,
                userId
            );

            var result = await _mediator.Send(command, ct);

            await SendAsync(new CreateSensorReadingResponseDto
            {
                Id = result.SensorReadingId,
                Success = result.Success,
                Message = result.Message
            }, result.Success ? 201 : 500, ct);
        }
    }
}