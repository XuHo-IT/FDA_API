using FDAAPI.App.Common.DTOs;

namespace FDAAPI.App.Common.Models.Routing;

public class GraphHopperRouteResponse
{
    public List<GraphHopperPath> Paths { get; set; } = new();
}

public class GraphHopperPath
{
    public double Distance { get; set; }
    public long Time { get; set; }
    public GeoJsonGeometry Geometry { get; set; } = new();
    public List<GraphHopperInstruction> Instructions { get; set; } = new();
}

public class GraphHopperInstruction
{
    public double Distance { get; set; }
    public int Time { get; set; }
    public int Sign { get; set; }
    public string Text { get; set; } = string.Empty;
}
