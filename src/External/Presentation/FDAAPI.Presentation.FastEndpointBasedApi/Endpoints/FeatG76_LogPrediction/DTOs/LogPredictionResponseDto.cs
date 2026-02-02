using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG76_LogPrediction.DTOs
{
    public class LogPredictionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public LogPredictionDataDto? Data { get; set; }
    }
}

