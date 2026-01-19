using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.FloodEvents;

namespace FDAAPI.App.FeatG66_FloodEventDelete
{
    public class DeleteFloodEventResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FloodEventStatusCode StatusCode { get; set; }
    }
}

