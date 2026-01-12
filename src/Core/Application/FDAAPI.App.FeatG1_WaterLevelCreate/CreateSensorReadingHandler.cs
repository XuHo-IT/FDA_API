using FDAAPI.App.Common.Models.SensorReadings;
using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG1_SensorReadingCreate
{
    public class CreateSensorReadingHandler : IRequestHandler<CreateSensorReadingRequest, CreateSensorReadingResponse>
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;

        public CreateSensorReadingHandler(ISensorReadingRepository sensorReadingRepository)
        {
            _sensorReadingRepository = sensorReadingRepository;
        }

        public async Task<CreateSensorReadingResponse> Handle(CreateSensorReadingRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var sensorReading = new SensorReading
                {
                    Id = Guid.NewGuid(),
                    StationId = request.StationId,
                    Value = request.Value,
                    Distance = request.Distance,
                    SensorHeight = request.SensorHeight,
                    Unit = request.Unit,
                    Status = request.Status,
                    MeasuredAt = request.MeasuredAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedByUserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = request.CreatedByUserId
                };

                await _sensorReadingRepository.CreateAsync(sensorReading, cancellationToken);

                return new CreateSensorReadingResponse
                {
                    Success = true,
                    Message = "Sensor reading created successfully",
                    StatusCode = SensorReadingStatusCode.Success,
                    SensorReadingId = sensorReading.Id
                };
            }
            catch (Exception ex)
            {
                return new CreateSensorReadingResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = SensorReadingStatusCode.UnknownError
                };
            }
        }
    }
}