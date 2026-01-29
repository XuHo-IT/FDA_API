using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class FloodWarningDto
    {
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int SeverityLevel { get; set; }
        public double WaterLevel { get; set; }
        public string Unit { get; set; } = "cm";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public GeoJsonGeometry FloodPolygon { get; set; } = new();
        public decimal DistanceFromRouteMeters { get; set; }
    }

}
