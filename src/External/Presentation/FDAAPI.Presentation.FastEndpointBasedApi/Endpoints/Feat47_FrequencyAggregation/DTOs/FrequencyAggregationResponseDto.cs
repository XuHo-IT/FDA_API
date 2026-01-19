using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat47_FrequencyAggregation.DTOs
{
    public class FrequencyAggregationResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public JobRunDto? Data { get; set; }
    }

    public class JobRunDto
    {
        public Guid JobRunId { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
    }
}

