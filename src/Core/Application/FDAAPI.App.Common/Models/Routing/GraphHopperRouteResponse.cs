using System.Text.Json.Serialization;
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

    /// <summary>
    /// Raw geometry from GraphHopper (LineString with 2D coordinates).
    /// Use GetFlatCoordinates() for flat [lng,lat,lng,lat,...] access.
    /// </summary>
    [JsonPropertyName("points")]
    public LineStringGeometry Points { get; set; } = new();

    public List<GraphHopperInstruction> Instructions { get; set; } = new();

    /// <summary>
    /// Convert to GeoJsonGeometry (flat coordinates) for flood analysis and response mapping.
    /// </summary>
    public GeoJsonGeometry ToGeoJsonGeometry()
    {
        var flat = new List<decimal>();
        foreach (var coord in Points.Coordinates)
        {
            if (coord.Length >= 2)
            {
                flat.Add((decimal)coord[0]); // lng
                flat.Add((decimal)coord[1]); // lat
            }
        }

        return new GeoJsonGeometry
        {
            Type = "LineString",
            Coordinates = flat.ToArray()
        };
    }
}

/// <summary>
/// GraphHopper returns geometry as { "type": "LineString", "coordinates": [[lng,lat], ...] }
/// when points_encoded=false
/// </summary>
public class LineStringGeometry
{
    public string Type { get; set; } = "LineString";
    public double[][] Coordinates { get; set; } = Array.Empty<double[]>();
}

public class GraphHopperInstruction
{
    public double Distance { get; set; }
    public int Time { get; set; }
    public int Sign { get; set; }
    public string Text { get; set; } = string.Empty;
}
