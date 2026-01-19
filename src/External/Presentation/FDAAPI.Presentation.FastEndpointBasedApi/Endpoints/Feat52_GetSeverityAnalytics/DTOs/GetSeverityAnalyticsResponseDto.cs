using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat52_GetSeverityAnalytics.DTOs
{
    public class GetSeverityAnalyticsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public SeverityAnalyticsDto? Data { get; set; }
    }

    public class SeverityAnalyticsDto
    {
        public Guid? AdministrativeAreaId { get; set; }
        public string? AdministrativeAreaName { get; set; }
        public string BucketType { get; set; } = string.Empty;
        public List<SeverityDataPointDto> DataPoints { get; set; } = new();
    }

    public class SeverityDataPointDto
    {
        public DateTime TimeBucket { get; set; }
        public decimal? MaxLevel { get; set; }
        public decimal? AvgLevel { get; set; }
        public decimal? MinLevel { get; set; }
        public int DurationHours { get; set; }
        public int ReadingCount { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}

