using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Routing;

namespace FDAAPI.App.Common.Services.Mapping
{
    public interface ISafeRouteMapper
    {
        RouteDto MapToRouteDto(GraphHopperPath path, List<FloodWarningDto> warnings);
    }
}
