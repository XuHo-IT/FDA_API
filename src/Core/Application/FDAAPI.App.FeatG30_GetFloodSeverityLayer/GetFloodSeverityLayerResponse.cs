using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Map;

namespace FDAAPI.App.FeatG30_GetFloodSeverityLayer
{
    public class GetFloodSeverityLayerResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GetFloodSeverityLayerResponseStatusCode StatusCode { get; set; }
        public GeoJsonFeatureCollection? Data { get; set; }
    }
}
