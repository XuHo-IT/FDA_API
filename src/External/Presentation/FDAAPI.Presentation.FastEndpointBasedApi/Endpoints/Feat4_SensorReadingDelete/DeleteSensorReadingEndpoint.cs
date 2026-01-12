using FastEndpoints;
using FDAAPI.App.Common.Models.SensorReadings;
using FDAAPI.App.FeatG4_SensorReadingDelete;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat4_SensorReadingDelete.DTOs;
using MediatR;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat4_SensorReadingDelete
{
    public class DeleteSensorReadingEndpoint : Endpoint<DeleteSensorReadingRequestDto, DeleteSensorReadingResponseDto>
    {
        private readonly IMediator _mediator;

        public DeleteSensorReadingEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/api/v1/sensor-readings/{id}");
            Policies("Admin");  // Only admin can delete
            Summary(s => {
                s.Summary = "Delete a sensor reading";
                s.Description = "Permanently deletes a sensor reading record by ID";
            });
            Tags("SensorReadings", "IoT");
        }

        public override async Task HandleAsync(DeleteSensorReadingRequestDto req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new DeleteSensorReadingResponseDto
                {
                    Success = false,
                    Message = "Unauthorized: Could not identify user"
                }, 401, ct);
                return;
            }

            var command = new DeleteSensorReadingRequest(req.Id, userId);
            var result = await _mediator.Send(command, ct);

            var statusCode = result.StatusCode switch
            {
                SensorReadingStatusCode.Success => 200,
                SensorReadingStatusCode.NotFound => 404,
                _ => 500
            };

            await SendAsync(new DeleteSensorReadingResponseDto
            {
                Success = result.Success,
                Message = result.Message
            }, statusCode, ct);
        }
    }
}