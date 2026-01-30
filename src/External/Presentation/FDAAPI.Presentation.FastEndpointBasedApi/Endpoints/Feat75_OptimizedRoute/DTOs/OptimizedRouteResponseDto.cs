using FDAAPI.App.FeatG75_OptimizedRoute;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat75_OptimizedRoute.DTOs
{
    public class OptimizedRouteResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public OptimizedRouteGeoJsonData? Data { get; set; }
    }
}
