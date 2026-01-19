using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.FloodEvents;

namespace FDAAPI.App.FeatG64_FloodEventGet
{
    public class GetFloodEventResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FloodEventStatusCode StatusCode { get; set; }
        public FloodEventDto? FloodEvent { get; set; }
    }
}

