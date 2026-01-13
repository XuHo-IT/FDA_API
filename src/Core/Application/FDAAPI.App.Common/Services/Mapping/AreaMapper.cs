using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FDAAPI.App.Common.Services.Mapping
{
    public class AreaMapper : IAreaMapper
    {
        public AreaDto MapToDto(Area area)
        {
            if (area == null)
            {
                throw new ArgumentNullException(nameof(area));
            }

            return new AreaDto
            {
                Id = area.Id,
                UserId = area.UserId,
                Name = area.Name,
                Latitude = area.Latitude,
                Longitude = area.Longitude,
                RadiusMeters = area.RadiusMeters,
                AddressText = area.AddressText,
                CreatedAt = area.CreatedAt,
                UpdatedAt = area.UpdatedAt
            };
        }

        public List<AreaDto> MapToDtoList(IEnumerable<Area> areas)
        {
            if (areas == null)
            {
                return new List<AreaDto>();
            }

            return areas.Select(MapToDto).ToList();
        }
    }
}

