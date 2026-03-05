using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat51_GetFrequencyAnalytics.DTOs
{
    public class GetFrequencyAnalyticsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public FrequencyAnalyticsDto? Data { get; set; }
    }

    public class FrequencyAnalyticsDto
    {
        public Guid? AdministrativeAreaId { get; set; }
        public string? AdministrativeAreaName { get; set; }
        public string BucketType { get; set; } = string.Empty;
        public List<FrequencyDataPointDto> DataPoints { get; set; } = new();
    }

    public class FrequencyDataPointDto
    {
        public DateTime TimeBucket { get; set; }
        public int EventCount { get; set; }
        public int ExceedCount { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}

