using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper interface for Station entity to StationDto
    /// </summary>
    public interface IStationMapper
    {
        /// <summary>
        /// Maps a Station entity to StationDto
        /// </summary>
        /// <param name="station">The station entity</param>
        /// <returns>StationDto</returns>
        StationDto MapToDto(Station station);

        /// <summary>
        /// Maps a collection of Station entities to StationDto list
        /// </summary>
        /// <param name="stations">Collection of station entities</param>
        /// <returns>List of StationDto</returns>
        List<StationDto> MapToDtoList(IEnumerable<Station> stations);
    }
}

