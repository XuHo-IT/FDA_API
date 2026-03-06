using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat84_FloodReportGetNearby.DTOs
{
    public sealed class GetNearbyFloodReportsRequestDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusMeters { get; set; } = 500;
        public int Hours { get; set; } = 2;
    }

    public sealed class GetNearbyFloodReportsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
        public string ConsensusLevel { get; set; } = "none";
        public string ConsensusMessage { get; set; } = string.Empty;
        public List<NearbyFloodReportItemDto> Reports { get; set; } = new();
    }

    public sealed class NearbyFloodReportItemDto
    {
        public Guid Id { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Severity { get; set; } = "medium";
        public DateTime CreatedAt { get; set; }
        public double DistanceMeters { get; set; }
    }
}
