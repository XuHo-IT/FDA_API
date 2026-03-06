using System;
using System.Collections.Generic;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// GeoJSON FeatureCollection DTO
    /// </summary>
    public class GeoJsonFeatureCollection
    {
        public string Type { get; set; } = "FeatureCollection";
        public List<GeoJsonFeature> Features { get; set; } = new();
        public object? Metadata { get; set; }
    }

    /// <summary>
    /// GeoJSON Feature DTO
    /// </summary>
    public class GeoJsonFeature
    {
        public string Type { get; set; } = "Feature";
        public object Geometry { get; set; } = new GeoJsonGeometry();
        public object? Properties { get; set; }
    }

    /// <summary>
    /// GeoJSON Point Geometry DTO
    /// </summary>
    public class GeoJsonGeometry
    {
        public string Type { get; set; } = "Point";
        public decimal[] Coordinates { get; set; } = Array.Empty<decimal>();
    }

    /// <summary>
    /// GeoJSON LineString Geometry DTO — for road segment gradient features
    /// </summary>
    public class LineStringGeometry
    {
        public string Type { get; set; } = "LineString";
        public decimal[][] Coordinates { get; set; } = Array.Empty<decimal[]>();
    }

    /// <summary>
    /// Bounding box for map area
    /// </summary>
    public class BoundingBox
    {
        public decimal MinLat { get; set; }
        public decimal MinLng { get; set; }
        public decimal MaxLat { get; set; }
        public decimal MaxLng { get; set; }
    }
}
