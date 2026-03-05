using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG78_GetPredictionAccuracyStats.DTOs
{
    public class GetPredictionAccuracyStatsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public PredictionAccuracyStatsDto? Data { get; set; }
    }
}

