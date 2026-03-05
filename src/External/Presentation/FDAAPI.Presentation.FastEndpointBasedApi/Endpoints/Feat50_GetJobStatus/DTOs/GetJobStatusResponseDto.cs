using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat50_GetJobStatus.DTOs
{
    public class GetJobStatusResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public JobStatusDto? Data { get; set; }
    }

    public class JobStatusDto
    {
        public Guid JobRunId { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? ExecutionTimeMs { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsCreated { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

