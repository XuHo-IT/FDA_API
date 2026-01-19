using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper implementation for FloodEvent entity to FloodEventDto
    /// </summary>
    public class FloodEventMapper : IFloodEventMapper
    {
        public FloodEventDto MapToDto(FloodEvent floodEvent)
        {
            if (floodEvent == null)
            {
                throw new ArgumentNullException(nameof(floodEvent));
            }

            return new FloodEventDto
            {
                Id = floodEvent.Id,
                AdministrativeAreaId = floodEvent.AdministrativeAreaId,
                AdministrativeAreaName = floodEvent.AdministrativeArea?.Name,
                StartTime = floodEvent.StartTime,
                EndTime = floodEvent.EndTime,
                PeakLevel = floodEvent.PeakLevel,
                DurationHours = floodEvent.DurationHours,
                CreatedAt = floodEvent.CreatedAt
            };
        }

        public List<FloodEventDto> MapToDtoList(IEnumerable<FloodEvent> floodEvents)
        {
            if (floodEvents == null)
            {
                return new List<FloodEventDto>();
            }

            return floodEvents.Select(MapToDto).ToList();
        }
    }
}

