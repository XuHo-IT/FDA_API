namespace FDAAPI.App.FeatG31_GetMapCurrentStatus
{
    public class GetMapCurrentStatusResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GetMapCurrentStatusResponseStatusCode StatusCode { get; set; }
        public GeoJsonFeatureCollection? Data { get; set; }
    }

    // GeoJSON Format Classes
    public class GeoJsonFeatureCollection
    {
        public string Type { get; set; } = "FeatureCollection";
        public List<GeoJsonFeature> Features { get; set; } = new();
        public object? Metadata { get; set; }
    }

    public class GeoJsonFeature
    {
        public string Type { get; set; } = "Feature";
        public GeoJsonGeometry Geometry { get; set; } = new();
        public object? Properties { get; set; }
    }

    public class GeoJsonGeometry
    {
        public string Type { get; set; } = "Point";
        public decimal[] Coordinates { get; set; } = Array.Empty<decimal>();
    }

    // Station with latest sensor reading
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
    }
}