using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Map;

namespace FDAAPI.App.FeatG31_GetMapCurrentStatus
{
    public class GetMapCurrentStatusResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GetMapCurrentStatusResponseStatusCode StatusCode { get; set; }
        public GeoJsonFeatureCollection? Data { get; set; }
    }
}