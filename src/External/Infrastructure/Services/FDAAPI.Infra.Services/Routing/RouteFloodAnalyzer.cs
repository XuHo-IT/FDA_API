using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Infra.Services.Routing
{
    public class RouteFloodAnalyzer : IRouteFloodAnalyzer
    {
        // Radius (meters) for avoid polygon around flooded station
        private const double CRITICAL_RADIUS_METERS = 150;
        private const double WARNING_RADIUS_METERS = 100;
        private const double CAUTION_RADIUS_METERS = 50;

        // Number of points to approximate circle polygon
        private const int CIRCLE_POINTS = 32;

        public List<FloodPolygon> BuildFloodPolygons(
            List<Station> stations,
            Dictionary<Guid, SensorReading> latestReadings)
        {
            var floodPolygons = new List<FloodPolygon>();

            foreach (var station in stations)
            {
                if (!station.Latitude.HasValue || !station.Longitude.HasValue)
                    continue;

                if (!latestReadings.TryGetValue(station.Id, out var reading))
                    continue;

                var (severity, level) = CalculateFloodSeverity(reading.Value, station);

                // Only create polygons for warning+ severity
                if (level < 2) // Skip safe(0) and caution(1)
                    continue;

                var radiusMeters = level switch
                {
                    3 => CRITICAL_RADIUS_METERS,
                    2 => WARNING_RADIUS_METERS,
                    _ => CAUTION_RADIUS_METERS
                };

                var circleGeometry = CreateCirclePolygon(
                    (double)station.Latitude.Value,
                    (double)station.Longitude.Value,
                    radiusMeters);

                floodPolygons.Add(new FloodPolygon
                {
                    StationId = station.Id,
                    StationName = station.Name,
                    StationCode = station.Code,
                    Latitude = station.Latitude.Value,
                    Longitude = station.Longitude.Value,
                    WaterLevel = reading.Value,
                    Severity = severity,
                    SeverityLevel = level,
                    Geometry = circleGeometry
                });
            }

            return floodPolygons;
        }

        public List<FloodWarningDto> AnalyzeRoute(
            GeoJsonGeometry routeGeometry,
            List<FloodPolygon> floodPolygons)
        {
            var warnings = new List<FloodWarningDto>();

            foreach (var polygon in floodPolygons)
            {
                var distance = CalculateHaversineDistance(
                    routeGeometry, polygon.Latitude, polygon.Longitude);

                // Report warning if route passes within 1km of flooded station
                if (distance <= 1000)
                {
                    warnings.Add(new FloodWarningDto
                    {
                        StationId = polygon.StationId,
                        StationName = polygon.StationName,
                        StationCode = polygon.StationCode,
                        Severity = polygon.Severity,
                        SeverityLevel = polygon.SeverityLevel,
                        WaterLevel = polygon.WaterLevel,
                        Unit = "cm",
                        Latitude = polygon.Latitude,
                        Longitude = polygon.Longitude,
                        FloodPolygon = polygon.Geometry,
                        DistanceFromRouteMeters = (decimal)distance
                    });
                }
            }

            return warnings;
        }

        public RouteSafetyStatus CalculateSafetyStatus(List<FloodWarningDto> warnings)
        {
            if (!warnings.Any())
                return RouteSafetyStatus.Safe;

            if (warnings.Any(w => w.SeverityLevel >= 3))
                return RouteSafetyStatus.Dangerous;

            if (warnings.Any(w => w.SeverityLevel >= 2))
                return RouteSafetyStatus.Caution;

            return RouteSafetyStatus.Safe;
        }

        /// <summary>
        /// Calculate flood severity matching FeatG31 logic exactly.
        /// Uses station custom thresholds if available, otherwise defaults.
        /// </summary>
        private (string severity, int level) CalculateFloodSeverity(
            double waterLevel, Station station)
        {
            double waterLevelInMeters = waterLevel / 100.0;

            // Use station-specific thresholds if available (like FeatG55)
            if (station.ThresholdCritical.HasValue && station.ThresholdWarning.HasValue)
            {
                var criticalThreshold = (double)station.ThresholdCritical.Value;
                var warningThreshold = (double)station.ThresholdWarning.Value;

                if (waterLevelInMeters >= criticalThreshold)
                    return ("critical", 3);
                if (waterLevelInMeters >= warningThreshold)
                    return ("warning", 2);
                if (waterLevelInMeters >= warningThreshold * 0.5)
                    return ("caution", 1);
                return ("safe", 0);
            }

            // Default thresholds (matching FeatG31)
            if (waterLevelInMeters >= 0.4)
                return ("critical", 3);
            if (waterLevelInMeters >= 0.2)
                return ("warning", 2);
            if (waterLevelInMeters >= 0.1)
                return ("caution", 1);
            return ("safe", 0);
        }

        /// <summary>
        /// Create a circular GeoJSON Polygon around a point.
        /// Used as avoid_polygon for GraphHopper.
        /// </summary>
        private GeoJsonGeometry CreateCirclePolygon(
            double centerLat, double centerLng, double radiusMeters)
        {
            var coordinates = new List<decimal>();

            for (int i = 0; i <= CIRCLE_POINTS; i++)
            {
                var angle = (2 * Math.PI * i) / CIRCLE_POINTS;

                // Approximate offset in degrees
                var dLat = (radiusMeters / 111320.0) * Math.Cos(angle);
                var dLng = (radiusMeters / (111320.0 * Math.Cos(centerLat * Math.PI / 180))) * Math.Sin(angle);

                coordinates.Add((decimal)(centerLng + dLng)); // lng first (GeoJSON)
                coordinates.Add((decimal)(centerLat + dLat));  // lat second
            }

            return new GeoJsonGeometry
            {
                Type = "Polygon",
                Coordinates = coordinates.ToArray()
            };
        }

        /// <summary>
        /// Calculate minimum distance from any point on the route to a station.
        /// Iterates all coordinate pairs [lng, lat] in the flat array.
        /// </summary>
        private double CalculateHaversineDistance(
            GeoJsonGeometry routeGeometry, decimal stationLat, decimal stationLng)
        {
            if (routeGeometry.Coordinates.Length < 2)
                return double.MaxValue;

            var minDistance = double.MaxValue;
            var stLat = (double)stationLat;
            var stLng = (double)stationLng;

            // Coordinates is flat: [lng0, lat0, lng1, lat1, ...]
            for (int i = 0; i < routeGeometry.Coordinates.Length - 1; i += 2)
            {
                var routeLng = (double)routeGeometry.Coordinates[i];
                var routeLat = (double)routeGeometry.Coordinates[i + 1];

                var distance = HaversineDistance(routeLat, routeLng, stLat, stLng);
                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        private double HaversineDistance(
            double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth radius in meters
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
