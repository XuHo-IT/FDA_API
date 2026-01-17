using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat46_GetFloodStatistics.DTOs
{
    public class GetFloodStatisticsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FloodStatisticsDto? Data { get; set; }
    }
}
