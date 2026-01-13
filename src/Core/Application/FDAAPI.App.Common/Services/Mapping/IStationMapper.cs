using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using System.Collections.Generic;

namespace FDAAPI.App.Common.Services.Mapping
{
    public interface IStationMapper
    {
        StationDto MapToDto(Station station);
        List<StationDto> MapToDtoList(IEnumerable<Station> stations);
    }
}

