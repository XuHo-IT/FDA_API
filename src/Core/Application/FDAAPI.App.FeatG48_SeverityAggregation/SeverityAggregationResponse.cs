using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Analytics;
using System;

namespace FDAAPI.App.FeatG48_SeverityAggregation
{
    public class SeverityAggregationResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AnalyticsStatusCode StatusCode { get; set; }
        public JobRunDto? Data { get; set; }
    }
}

