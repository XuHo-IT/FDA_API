namespace FDAAPI.App.FeatG30_GetFloodSeverityLayer
{
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
        public object Properties { get; set; } = new { };
    }

    public class GeoJsonGeometry
    {
        public string Type { get; set; } = "Point";
        public decimal[] Coordinates { get; set; } = System.Array.Empty<decimal>();
    }

    public class BoundingBox
    {
        public decimal MinLat { get; set; }
        public decimal MinLng { get; set; }
        public decimal MaxLat { get; set; }
        public decimal MaxLng { get; set; }
    }
}
