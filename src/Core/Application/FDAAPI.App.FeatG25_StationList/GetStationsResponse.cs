using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;

namespace FDAAPI.App.FeatG25_StationList
{
    public class GetStationsResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public StationStatusCode StatusCode { get; set; }
        public IEnumerable<StationDto> Stations { get; set; } = Enumerable.Empty<StationDto>();
        public int TotalCount { get; set; }
    }
}

