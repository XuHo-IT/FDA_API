using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Routing;

namespace FDAAPI.App.Common.Services.Mapping;

public class SafeRouteMapper : ISafeRouteMapper
{
    public object BuildRouteFeature(
        GraphHopperPath path,
        List<FloodWarningDto> warnings,
        string featureName)
    {
        return new
        {
            type = "Feature",
            geometry = new
            {
                type = "LineString",
                coordinates = To2DCoordinates(path.Points.Coordinates)
            },
            properties = new
            {
                name = featureName,
                distanceMeters = path.Distance,
                durationSeconds = (int)(path.Time / 1000),
                floodRiskScore = CalculateFloodRiskScore(warnings),
                instructions = path.Instructions.Select(i => new
                {
                    distance = i.Distance,
                    time = i.Time,
                    text = i.Text
                }).ToArray()
            }
        };
    }

    public object BuildFloodZoneFeature(FloodWarningDto warning)
    {
        return new
        {
            type = "Feature",
            geometry = new
            {
                type = "Polygon",
                coordinates = new[] { FlatToPolygonRing(warning.FloodPolygon.Coordinates) }
            },
            properties = new
            {
                name = "floodZone",
                stationId = warning.StationId,
                stationCode = warning.StationCode,
                stationName = warning.StationName,
                severity = warning.Severity,
                severityLevel = warning.SeverityLevel,
                waterLevel = warning.WaterLevel,
                unit = warning.Unit,
                latitude = warning.Latitude,
                longitude = warning.Longitude,
                distanceFromRouteMeters = warning.DistanceFromRouteMeters
            }
        };
    }

    /// <summary>
    /// Convert GraphHopper double[][] to GeoJSON 2D: [[lng, lat], ...]
    /// </summary>
    private double[][] To2DCoordinates(double[][] coords)
    {
        return coords.Select(c => new[] { c[0], c[1] }).ToArray();
    }

    /// <summary>
    /// Convert flat decimal[] [lng,lat,lng,lat,...] to polygon ring [[lng,lat], ...]
    /// </summary>
    private decimal[][] FlatToPolygonRing(decimal[] flat)
    {
        var ring = new List<decimal[]>();
        for (int i = 0; i < flat.Length - 1; i += 2)
        {
            ring.Add(new[] { flat[i], flat[i + 1] });
        }
        return ring.ToArray();
    }

    private decimal CalculateFloodRiskScore(List<FloodWarningDto> warnings)
    {
        if (!warnings.Any()) return 0;

        var criticalCount = warnings.Count(w => w.Severity == "critical");
        var warningCount = warnings.Count(w => w.Severity == "warning");
        var cautionCount = warnings.Count(w => w.Severity == "caution");

        var score = (criticalCount * 40) + (warningCount * 20) + (cautionCount * 10);
        return Math.Min(100, score);
    }
}
