using FDAAPI.App.Common.DTOs;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG77_GetPredictionComparisons.DTOs
{
    public class GetPredictionComparisonsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public GetPredictionComparisonsDataDto? Data { get; set; }
    }

    public class GetPredictionComparisonsDataDto
    {
        public int Total { get; set; }
        public List<PredictionLogDto> Items { get; set; } = new();
        public PredictionComparisonSummaryDto Summary { get; set; } = new();
    }
}

