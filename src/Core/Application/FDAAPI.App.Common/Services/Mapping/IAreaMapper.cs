using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using System.Collections.Generic;

namespace FDAAPI.App.Common.Services.Mapping
{
    public interface IAreaMapper
    {
        AreaDto MapToDto(Area area);
        List<AreaDto> MapToDtoList(IEnumerable<Area> areas);
    }
}

