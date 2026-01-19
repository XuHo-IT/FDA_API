using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.FloodEvents;

namespace FDAAPI.App.FeatG65_FloodEventUpdate
{
    public class UpdateFloodEventResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FloodEventStatusCode StatusCode { get; set; }
    }
}

