using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG76_LogPrediction;

namespace FDAAPI.App.FeatG78_GetPredictionAccuracyStats
{
    public class GetPredictionAccuracyStatsResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PredictionLogStatusCode StatusCode { get; set; }
        public PredictionAccuracyStatsDto? Data { get; set; }
    }
}

