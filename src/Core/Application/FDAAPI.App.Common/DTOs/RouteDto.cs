using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.DTOs
{
    public class RouteDto
    {
        public GeoJsonGeometry Geometry { get; set; } = new();
        public decimal DistanceMeters { get; set; }
        public int DurationSeconds { get; set; }
        public List<RouteInstructionDto> Instructions { get; set; } = new();
        public decimal FloodRiskScore { get; set; }
    }

    public class RouteInstructionDto
    {
        public decimal Distance { get; set; }
        public int Time { get; set; }
        public string Text { get; set; } = string.Empty;
    }

}
