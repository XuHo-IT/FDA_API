using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat83_FloodReportList.DTOs
{
    public sealed class ListFloodReportsRequestDto
    {
        public string? Status { get; set; }
        public string? Severity { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public sealed class ListFloodReportsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public List<FloodReportListItemDto> Items { get; set; } = new();
    }

    public sealed class FloodReportListItemDto
    {
        public Guid Id { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string Severity { get; set; } = "medium";
        public int TrustScore { get; set; }
        public string Status { get; set; } = "published";
        public string ConfidenceLevel { get; set; } = "medium";
        public DateTime CreatedAt { get; set; }
        public List<FloodReportMediaDto> Media { get; set; } = new();

    }

    public sealed class FloodReportMediaDto
    {
        public Guid Id { get; set; }
        public string MediaType { get; set; } = "photo";
        public string MediaUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
