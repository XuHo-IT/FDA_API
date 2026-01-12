using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.App.Common.Models.SensorReadings;
using MediatR;

namespace FDAAPI.App.FeatG4_SensorReadingDelete
{
    public class DeleteSensorReadingHandler : IRequestHandler<DeleteSensorReadingRequest, DeleteSensorReadingResponse>
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;

        public DeleteSensorReadingHandler(ISensorReadingRepository sensorReadingRepository)
        {
            _sensorReadingRepository = sensorReadingRepository;
        }

        public async Task<DeleteSensorReadingResponse> Handle(DeleteSensorReadingRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if sensor reading exists
                var existingReading = await _sensorReadingRepository.GetByIdAsync(request.Id, cancellationToken);
                if (existingReading == null)
                {
                    return new DeleteSensorReadingResponse
                    {
                        Success = false,
                        Message = "Sensor reading not found",
                        StatusCode = SensorReadingStatusCode.NotFound
                    };
                }

                // Delete the sensor reading
                var deleted = await _sensorReadingRepository.DeleteAsync(request.Id, cancellationToken);

                if (!deleted)
                {
                    return new DeleteSensorReadingResponse
                    {
                        Success = false,
                        Message = "Failed to delete sensor reading",
                        StatusCode = SensorReadingStatusCode.UnknownError
                    };
                }

                return new DeleteSensorReadingResponse
                {
                    Success = true,
                    Message = "Sensor reading deleted successfully",
                    StatusCode = SensorReadingStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new DeleteSensorReadingResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = SensorReadingStatusCode.UnknownError
                };
            }
        }
    }
}