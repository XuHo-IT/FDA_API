using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat34_AreaStatusEvaluate.DTOs
{
    public class AreaStatusEvaluateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AreaStatusDto? Data { get; set; }
    }

    public class AreaStatusDto
    {
        public Guid AreaId { get; set; }
        public string Status { get; set; } = "Unknown";
        public int SeverityLevel { get; set; } = -1;
        public string Summary { get; set; } = string.Empty;
        public List<ContributingStationDto> ContributingStations { get; set; } = new();
        public DateTime EvaluatedAt { get; set; }
    }

    public class ContributingStationDto
    {
        public string StationCode { get; set; } = string.Empty;
        public double Distance { get; set; }
        public double WaterLevel { get; set; }
        public string Severity { get; set; } = string.Empty;
        public int Weight { get; set; }
    }
}

