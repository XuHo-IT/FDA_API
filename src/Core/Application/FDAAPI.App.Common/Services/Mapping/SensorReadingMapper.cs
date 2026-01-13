using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper implementation for SensorReading entity to SensorReadingDto
    /// </summary>
    public class SensorReadingMapper : ISensorReadingMapper
    {
        public SensorReadingDto MapToDto(SensorReading sensorReading)
        {
            if (sensorReading == null)
            {
                throw new ArgumentNullException(nameof(sensorReading));
            }

            return new SensorReadingDto
            {
                Id = sensorReading.Id,
                StationId = sensorReading.StationId,
                Value = sensorReading.Value,
                Distance = sensorReading.Distance,
                SensorHeight = sensorReading.SensorHeight,
                Unit = sensorReading.Unit ?? "cm",
                Status = sensorReading.Status,
                MeasuredAt = sensorReading.MeasuredAt,
                CreatedAt = sensorReading.CreatedAt,
                UpdatedAt = sensorReading.UpdatedAt
            };
        }

        public List<SensorReadingDto> MapToDtoList(IEnumerable<SensorReading> sensorReadings)
        {
            if (sensorReadings == null)
            {
                throw new ArgumentNullException(nameof(sensorReadings));
            }

            return sensorReadings.Select(MapToDto).ToList();
        }
    }
}

