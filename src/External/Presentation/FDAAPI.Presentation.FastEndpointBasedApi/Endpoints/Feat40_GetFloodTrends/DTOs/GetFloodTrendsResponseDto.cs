using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat40_GetFloodTrends.DTOs
{
    public class GetFloodTrendsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FloodTrendDto? Data { get; set; }
    }
}
