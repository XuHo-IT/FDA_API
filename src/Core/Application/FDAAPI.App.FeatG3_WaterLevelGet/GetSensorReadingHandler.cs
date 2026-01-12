using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.SensorReadings;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG3_SensorReadingGet
{
    public class GetSensorReadingHandler : IRequestHandler<GetSensorReadingRequest, GetSensorReadingResponse>
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;

        public GetSensorReadingHandler(ISensorReadingRepository sensorReadingRepository)
        {
            _sensorReadingRepository = sensorReadingRepository;
        }

        public async Task<GetSensorReadingResponse> Handle(GetSensorReadingRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var sensorReading = await _sensorReadingRepository.GetByIdAsync(request.Id, cancellationToken);

                if (sensorReading == null)
                {
                    return new GetSensorReadingResponse
                    {
                        Success = false,
                        Message = "Sensor reading not found",
                        StatusCode = SensorReadingStatusCode.NotFound
                    };
                }

                return new GetSensorReadingResponse
                {
                    Success = true,
                    Message = "Sensor reading retrieved successfully",
                    StatusCode = SensorReadingStatusCode.Success,
                    Data = new SensorReadingDto
                    {
                        Id = sensorReading.Id,
                        StationId = sensorReading.StationId,
                        Value = sensorReading.Value,
                        Distance = sensorReading.Distance,
                        SensorHeight = sensorReading.SensorHeight,
                        Unit = sensorReading.Unit,
                        Status = sensorReading.Status,
                        MeasuredAt = sensorReading.MeasuredAt,
                        CreatedAt = sensorReading.CreatedAt,
                        UpdatedAt = sensorReading.UpdatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                return new GetSensorReadingResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = SensorReadingStatusCode.UnknownError
                };
            }
        }
    }
}