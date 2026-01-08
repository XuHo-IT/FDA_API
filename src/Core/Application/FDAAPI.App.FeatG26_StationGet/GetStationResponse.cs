using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.FeatG26_StationGet
{
    public class GetStationResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StationStatusCode StatusCode { get; set; }
        public Station? Station { get; set; }
    }
}

