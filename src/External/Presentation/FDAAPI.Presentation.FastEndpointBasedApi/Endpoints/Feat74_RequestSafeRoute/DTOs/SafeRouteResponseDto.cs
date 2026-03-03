using FDAAPI.App.FeatG74_RequestSafeRoute;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat74_RequestSafeRoute.DTOs
{
    public class SafeRouteResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public SafeRouteGeoJsonData? Data { get; set; }
    }

}
