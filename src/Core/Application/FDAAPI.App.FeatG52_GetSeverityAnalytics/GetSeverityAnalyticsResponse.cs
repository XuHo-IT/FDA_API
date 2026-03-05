using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Analytics;
using System;
using System.Collections.Generic;

namespace FDAAPI.App.FeatG52_GetSeverityAnalytics
{
    public class GetSeverityAnalyticsResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AnalyticsStatusCode StatusCode { get; set; }
        public SeverityAnalyticsDto? Data { get; set; }
    }
}

