using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper implementation for AdministrativeArea entity to AdministrativeAreaDto
    /// </summary>
    public class AdministrativeAreaMapper : IAdministrativeAreaMapper
    {
        public AdministrativeAreaDto MapToDto(AdministrativeArea area)
        {
            if (area == null)
            {
                throw new ArgumentNullException(nameof(area));
            }

            return new AdministrativeAreaDto
            {
                Id = area.Id,
                Name = area.Name,
                Level = area.Level,
                ParentId = area.ParentId,
                Code = area.Code,
                Geometry = area.Geometry
            };
        }

        public List<AdministrativeAreaDto> MapToDtoList(IEnumerable<AdministrativeArea> areas)
        {
            if (areas == null)
            {
                return new List<AdministrativeAreaDto>();
            }

            return areas.Select(MapToDto).ToList();
        }
    }
}

