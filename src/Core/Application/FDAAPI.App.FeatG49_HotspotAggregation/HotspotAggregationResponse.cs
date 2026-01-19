using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Analytics;
using System;

namespace FDAAPI.App.FeatG49_HotspotAggregation
{
    public class HotspotAggregationResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AnalyticsStatusCode StatusCode { get; set; }
        public JobRunDto? Data { get; set; }
    }
}

