using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.App.FeatG74_RequestSafeRoute
{
    public class SafeRouteResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SafeRouteStatusCode StatusCode { get; set; }
        public SafeRouteGeoJsonData? Data { get; set; }
    }

    /// <summary>
    /// GeoJSON FeatureCollection response for map rendering.
    /// Features include: primaryRoute (LineString), alternativeRoutes (LineString), floodZones (Polygon).
    /// </summary>
    public class SafeRouteGeoJsonData
    {
        public string Type { get; set; } = "FeatureCollection";
        public List<object> Features { get; set; } = new();
        public SafeRouteMetadata Metadata { get; set; } = new();
    }

    public class SafeRouteMetadata
    {
        public RouteSafetyStatus SafetyStatus { get; set; }
        public int TotalFloodZones { get; set; }
        public int AlternativeRouteCount { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
