using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;

namespace FDAAPI.App.FeatG24_StationUpdate
{
    public class UpdateStationResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StationStatusCode StatusCode { get; set; }
    }
}
