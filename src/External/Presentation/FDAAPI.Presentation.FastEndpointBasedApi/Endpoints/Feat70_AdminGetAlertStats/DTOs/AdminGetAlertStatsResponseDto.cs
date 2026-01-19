using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat70_AdminGetAlertStats
{
    public class AdminGetAlertStatsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AlertStatsDataDto? Data { get; set; }
    }
}