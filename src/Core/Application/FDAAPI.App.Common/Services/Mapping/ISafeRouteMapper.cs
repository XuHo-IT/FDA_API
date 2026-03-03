using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.App.Common.Services.Mapping;

public interface ISafeRouteMapper
{
    /// <summary>
    /// Build a GeoJSON Feature for a route (LineString with 2D coordinates).
    /// </summary>
    object BuildRouteFeature(
        GraphHopperPath path,
        List<FloodWarningDto> warnings,
        string featureName);

    /// <summary>
    /// Build a GeoJSON Feature for a flood zone (Polygon with 2D coordinates).
    /// </summary>
    object BuildFloodZoneFeature(FloodWarningDto warning);
}
