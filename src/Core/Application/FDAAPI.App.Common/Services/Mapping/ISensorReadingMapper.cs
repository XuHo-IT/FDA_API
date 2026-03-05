using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper interface for SensorReading entity to SensorReadingDto
    /// </summary>
    public interface ISensorReadingMapper
    {
        /// <summary>
        /// Maps a SensorReading entity to SensorReadingDto
        /// </summary>
        /// <param name="sensorReading">The sensor reading entity</param>
        /// <returns>SensorReadingDto</returns>
        SensorReadingDto MapToDto(SensorReading sensorReading);

        /// <summary>
        /// Maps a collection of SensorReading entities to SensorReadingDto list
        /// </summary>
        /// <param name="sensorReadings">Collection of sensor reading entities</param>
        /// <returns>List of SensorReadingDto</returns>
        List<SensorReadingDto> MapToDtoList(IEnumerable<SensorReading> sensorReadings);
    }
}

