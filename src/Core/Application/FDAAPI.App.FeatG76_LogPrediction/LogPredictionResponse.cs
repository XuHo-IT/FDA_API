using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG76_LogPrediction
{
    public class LogPredictionResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PredictionLogStatusCode StatusCode { get; set; }
        public LogPredictionDataDto? Data { get; set; }
    }
}

