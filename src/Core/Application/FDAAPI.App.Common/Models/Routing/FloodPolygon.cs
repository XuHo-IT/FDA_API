using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.Common.Models.Routing
{
    public class FloodPolygon
    {
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double WaterLevel { get; set; }
        public string Severity { get; set; } = string.Empty;
        public int SeverityLevel { get; set; }
        public GeoJsonGeometry Geometry { get; set; } = new();
    }

}
