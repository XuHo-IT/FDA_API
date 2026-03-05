using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.SensorReadings;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG3_SensorReadingGet
{
    public class GetSensorReadingHandler : IRequestHandler<GetSensorReadingRequest, GetSensorReadingResponse>
    {
        private readonly ISensorReadingRepository _sensorReadingRepository;
        private readonly ISensorReadingMapper _sensorReadingMapper;

        public GetSensorReadingHandler(
            ISensorReadingRepository sensorReadingRepository,
            ISensorReadingMapper sensorReadingMapper)
        {
            _sensorReadingRepository = sensorReadingRepository;
            _sensorReadingMapper = sensorReadingMapper;
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

                // Use mapper to convert entity to DTO
                var sensorReadingDto = _sensorReadingMapper.MapToDto(sensorReading);

                return new GetSensorReadingResponse
                {
                    Success = true,
                    Message = "Sensor reading retrieved successfully",
                    StatusCode = SensorReadingStatusCode.Success,
                    Data = sensorReadingDto
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