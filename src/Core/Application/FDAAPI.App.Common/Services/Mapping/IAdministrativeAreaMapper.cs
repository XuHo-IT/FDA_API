using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper interface for AdministrativeArea entity to AdministrativeAreaDto
    /// </summary>
    public interface IAdministrativeAreaMapper
    {
        /// <summary>
        /// Maps an AdministrativeArea entity to AdministrativeAreaDto
        /// </summary>
        /// <param name="area">The administrative area entity</param>
        /// <returns>AdministrativeAreaDto</returns>
        AdministrativeAreaDto MapToDto(AdministrativeArea area);

        /// <summary>
        /// Maps a collection of AdministrativeArea entities to AdministrativeAreaDto list
        /// </summary>
        /// <param name="areas">Collection of administrative area entities</param>
        /// <returns>List of AdministrativeAreaDto</returns>
        List<AdministrativeAreaDto> MapToDtoList(IEnumerable<AdministrativeArea> areas);
    }
}

