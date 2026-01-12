using FastEndpoints;
using FDAAPI.App.Common.Models.SensorReadings;
using FDAAPI.App.FeatG3_SensorReadingGet;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3_SensorReadingGet.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3_SensorReadingGet
{
    public class GetSensorReadingEndpoint : Endpoint<GetSensorReadingRequestDto, GetSensorReadingResponseDto>
    {
        private readonly IMediator _mediator;

        public GetSensorReadingEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/api/v1/sensor-readings/{id}");
            AllowAnonymous();
            //Policies("Admin", "Authority", "User");
            Summary(s => {
                s.Summary = "Get sensor reading by ID";
                s.Description = "Retrieves a single sensor reading record";
            });
            Tags("SensorReadings", "IoT");
        }

        public override async Task HandleAsync(GetSensorReadingRequestDto req, CancellationToken ct)
        {
            var query = new GetSensorReadingRequest(req.Id);
            var result = await _mediator.Send(query, ct);

            var statusCode = result.StatusCode switch
            {
                SensorReadingStatusCode.Success => 200,
                SensorReadingStatusCode.NotFound => 404,
                _ => 500
            };

            var responseDto = new GetSensorReadingResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            if (result.Data != null)
            {
                responseDto.Data = new SensorReadingDataDto
                {
                    Id = result.Data.Id,
                    StationId = result.Data.StationId,
                    Value = result.Data.Value,
                    Distance = result.Data.Distance,
                    SensorHeight = result.Data.SensorHeight,
                    Unit = result.Data.Unit,
                    Status = result.Data.Status,
                    MeasuredAt = result.Data.MeasuredAt,
                    CreatedAt = result.Data.CreatedAt,
                    UpdatedAt = result.Data.UpdatedAt
                };
            }

            await SendAsync(responseDto, statusCode, ct);
        }
    }
}