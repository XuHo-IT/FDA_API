using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat79_FloodReportCreate.DTOs
{
    public sealed class CreateFloodReportResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public Guid? Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ConfidenceLevel { get; set; } = string.Empty;
        public int TrustScore { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}


