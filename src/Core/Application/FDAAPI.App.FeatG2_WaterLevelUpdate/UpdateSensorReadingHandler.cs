using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.App.Common.Models.SensorReadings;
using MediatR;

namespace FDAAPI.App.FeatG2_SensorReadingUpdate
{
    public class UpdateSensorReadingHandler : IRequestHandler<UpdateSensorReadingRequest, UpdateSensorReadingResponse>
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;

        public UpdateSensorReadingHandler(ISensorReadingRepository sensorReadingRepository)
        {
            _sensorReadingRepository = sensorReadingRepository;
        }

        public async Task<UpdateSensorReadingResponse> Handle(UpdateSensorReadingRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if sensor reading exists
                var existingReading = await _sensorReadingRepository.GetByIdAsync(request.Id, cancellationToken);
                if (existingReading == null)
                {
                    return new UpdateSensorReadingResponse
                    {
                        Success = false,
                        Message = "Sensor reading not found",
                        StatusCode = SensorReadingStatusCode.NotFound
                    };
                }

                // Update properties
                existingReading.StationId = request.StationId;
                existingReading.Value = request.Value;
                existingReading.Distance = request.Distance;
                existingReading.SensorHeight = request.SensorHeight;
                existingReading.Unit = request.Unit;
                existingReading.Status = request.Status;
                existingReading.MeasuredAt = request.MeasuredAt;
                existingReading.UpdatedAt = DateTime.UtcNow;
                existingReading.UpdatedBy = request.UpdatedByUserId;

                await _sensorReadingRepository.UpdateAsync(existingReading, cancellationToken);

                return new UpdateSensorReadingResponse
                {
                    Success = true,
                    Message = "Sensor reading updated successfully",
                    StatusCode = SensorReadingStatusCode.Success,
                    SensorReadingId = existingReading.Id
                };
            }
            catch (Exception ex)
            {
                return new UpdateSensorReadingResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = SensorReadingStatusCode.UnknownError
                };
            }
        }
    }
}