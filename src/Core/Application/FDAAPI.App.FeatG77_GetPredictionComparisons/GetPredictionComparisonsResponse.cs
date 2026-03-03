using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG76_LogPrediction;
using System.Collections.Generic;

namespace FDAAPI.App.FeatG77_GetPredictionComparisons
{
    public class GetPredictionComparisonsResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PredictionLogStatusCode StatusCode { get; set; }
        public PredictionComparisonsDataDto? Data { get; set; }
    }

    public class PredictionComparisonsDataDto
    {
        public int Total { get; set; }
        public List<PredictionLogDto> Items { get; set; } = new();
        public PredictionComparisonSummaryDto Summary { get; set; } = new();
    }
}

