using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat39_GetFloodHistory.DTOs
{
    /// <summary>
    /// Response DTO for flood history endpoint
    /// </summary>
    public class GetFloodHistoryResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FloodHistoryDto? Data { get; set; }
        public PaginationDto? Pagination { get; set; }
    }
}
