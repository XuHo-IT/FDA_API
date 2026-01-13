using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper implementation for Station entity to StationDto
    /// </summary>
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
                Code = station.Code ?? string.Empty,
                Name = station.Name ?? string.Empty,
                LocationDesc = station.LocationDesc ?? string.Empty,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                RoadName = station.RoadName ?? string.Empty,
                Direction = station.Direction ?? string.Empty,
                Status = station.Status ?? string.Empty,
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
                throw new ArgumentNullException(nameof(stations));
            }

            return stations.Select(MapToDto).ToList();
        }
    }
}

