using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.Common.Models.Routing;

public class GraphHopperRouteRequest
{
    public decimal[][] Points { get; set; } = Array.Empty<decimal[]>();
    public string Profile { get; set; } = "car";
    public List<GeoJsonGeometry>? AvoidPolygons { get; set; }
    public AlternativeRouteConfig? AlternativeRoute { get; set; }
    public bool PointsEncoded { get; set; } = false;
    public bool Instructions { get; set; } = true;
}

public class AlternativeRouteConfig
{
    public int MaxPaths { get; set; } = 3;
    public double MaxWeightFactor { get; set; } = 1.4;
}
