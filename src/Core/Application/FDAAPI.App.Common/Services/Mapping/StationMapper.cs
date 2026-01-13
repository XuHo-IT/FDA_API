using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FDAAPI.App.Common.Services.Mapping
{
    public class StationMapper : IStationMapper
    {
        public StationDto MapToDto(Station station)
        {
            if (station == null)
            {
                throw new ArgumentNullException(nameof(station));
            }

            return new StationDto
            {
                Id = station.Id,
                Code = station.Code,
                Name = station.Name,
                LocationDesc = station.LocationDesc,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                RoadName = station.RoadName,
                Direction = station.Direction,
                Status = station.Status,
                ThresholdWarning = station.ThresholdWarning,
                ThresholdCritical = station.ThresholdCritical,
                InstalledAt = station.InstalledAt,
                LastSeenAt = station.LastSeenAt,
                CreatedAt = station.CreatedAt,
                UpdatedAt = station.UpdatedAt
            };
        }

        public List<StationDto> MapToDtoList(IEnumerable<Station> stations)
        {
            if (stations == null)
            {
                return new List<StationDto>();
            }

            return stations.Select(MapToDto).ToList();
        }
    }
}

