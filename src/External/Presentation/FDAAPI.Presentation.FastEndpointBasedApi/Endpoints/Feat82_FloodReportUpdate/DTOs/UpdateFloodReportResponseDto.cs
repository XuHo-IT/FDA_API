using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat82_FloodReportUpdate.DTOs
{
    public class UpdateFloodReportResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? Id { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string Severity { get; set; } = "medium";
        public int TrustScore { get; set; }
        public string Status { get; set; } = "published";
        public string ConfidenceLevel { get; set; } = "medium";
        public string Priority { get; set; } = "normal";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<FloodReportMediaDto> Media { get; set; } = new();
    }

    public class FloodReportMediaDto
    {
        public Guid Id { get; set; }
        public string MediaType { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
