using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.App.FeatG75_OptimizedRoute
{
    public class OptimizedRouteResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SafeRouteStatusCode StatusCode { get; set; }
        public OptimizedRouteGeoJsonData? Data { get; set; }
    }

    public class OptimizedRouteGeoJsonData
    {
        public string Type { get; set; } = "FeatureCollection";
        public List<object> Features { get; set; } = new();
        public OptimizedRouteMetadata Metadata { get; set; } = new();
    }

    public class OptimizedRouteMetadata
    {
        public RouteSafetyStatus SafetyStatus { get; set; }
        public int TotalFloodZones { get; set; }
        public int AlternativeRouteCount { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public bool Cached { get; set; }
        public int WaypointCount { get; set; }
        public DateTime? DepartureTime { get; set; }
        public bool FloodTrendApplied { get; set; }
    }
}
