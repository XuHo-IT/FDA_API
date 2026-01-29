using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.App.Common.Services
{
    public interface IRouteFloodAnalyzer
    {
        /// <summary>
        /// Build flood polygons from stations with active flood readings
        /// </summary>
        List<FloodPolygon> BuildFloodPolygons(
            List<Station> stations,
            Dictionary<Guid, SensorReading> latestReadings);

        /// <summary>
        /// Analyze route against flood polygons and generate warnings
        /// </summary>
        List<FloodWarningDto> AnalyzeRoute(
            GeoJsonGeometry routeGeometry,
            List<FloodPolygon> floodPolygons);

        /// <summary>
        /// Calculate overall safety status from warnings
        /// </summary>
        RouteSafetyStatus CalculateSafetyStatus(List<FloodWarningDto> warnings);
    }

}
