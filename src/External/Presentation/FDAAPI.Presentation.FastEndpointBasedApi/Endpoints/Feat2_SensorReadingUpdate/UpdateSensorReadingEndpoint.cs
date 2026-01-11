using FastEndpoints;
using FDAAPI.App.Common.Models.SensorReadings;
using FDAAPI.App.FeatG2_SensorReadingUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2_SensorReadingUpdate.DTOs;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2_SensorReadingUpdate
{
    public class UpdateSensorReadingEndpoint : Endpoint<UpdateSensorReadingRequestDto, UpdateSensorReadingResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateSensorReadingEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/api/v1/sensor-readings/{id}");
            Policies("Admin", "Authority");
            Summary(s => {
                s.Summary = "Update an existing sensor reading";
                s.Description = "Updates sensor reading data by ID";
            });
            Tags("SensorReadings", "IoT");
        }

        public override async Task HandleAsync(UpdateSensorReadingRequestDto req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new UpdateSensorReadingResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify user"
                }, 401, ct);
                return;
            }

            var command = new UpdateSensorReadingRequest(
                req.Id,
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

            var statusCode = result.StatusCode switch
            {
                SensorReadingStatusCode.Success => 200,
                SensorReadingStatusCode.NotFound => 404,
                SensorReadingStatusCode.ValidationError => 400,
                _ => 500
            };

            await SendAsync(new UpdateSensorReadingResponseDto
            {
                Id = result.SensorReadingId,
                Success = result.Success,
                Message = result.Message
            }, statusCode, ct);
        }
    }
}