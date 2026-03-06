using System;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Station with flood status and latest sensor reading
    /// </summary>
    public class StationFloodStatus
    {
        public Guid StationId { get; set; }
        public string StationCode { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string LocationDesc { get; set; } = string.Empty;
        public string RoadName { get; set; } = string.Empty;

        // Latest sensor reading data
        public double? WaterLevel { get; set; }
        public double? Distance { get; set; }
        public double? SensorHeight { get; set; }
        public string Unit { get; set; } = "cm";
        public DateTime? MeasuredAt { get; set; }

        // Flood severity calculation
        public string Severity { get; set; } = "unknown";  // safe, caution, warning, critical, unknown
        public int SeverityLevel { get; set; } = -1;  // 0: safe, 1: caution, 2: warning, 3: critical, -1: no data

        // Station status
        public string StationStatus { get; set; } = string.Empty;
        public DateTimeOffset? LastSeenAt { get; set; }

        // Coordinates (for road segment generation)
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
