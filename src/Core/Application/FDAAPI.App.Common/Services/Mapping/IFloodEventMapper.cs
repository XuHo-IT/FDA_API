using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper interface for FloodEvent entity to FloodEventDto
    /// </summary>
    public interface IFloodEventMapper
    {
        /// <summary>
        /// Maps a FloodEvent entity to FloodEventDto
        /// </summary>
        /// <param name="floodEvent">The flood event entity</param>
        /// <returns>FloodEventDto</returns>
        FloodEventDto MapToDto(FloodEvent floodEvent);

        /// <summary>
        /// Maps a collection of FloodEvent entities to FloodEventDto list
        /// </summary>
        /// <param name="floodEvents">Collection of flood event entities</param>
        /// <returns>List of FloodEventDto</returns>
        List<FloodEventDto> MapToDtoList(IEnumerable<FloodEvent> floodEvents);
    }
}

