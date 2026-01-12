using FDAAPI.App.FeatG31_GetMapCurrentStatus;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat31_GetMapCurrentStatus.DTOs
{
    public class GetMapCurrentStatusResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GeoJsonFeatureCollection? Data { get; set; }
    }
}