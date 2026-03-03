using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat55_AdministrativeAreasEvaluate.DTOs
{
    public class AdministrativeAreasEvaluateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdministrativeAreaStatusDto? Data { get; set; }
    }

    public class AdministrativeAreaStatusDto
    {
        public Guid AdministrativeAreaId { get; set; }
        public string Status { get; set; } = "Unknown";
        public int SeverityLevel { get; set; } = -1;
        public string Summary { get; set; } = string.Empty;
        public List<ContributingStationDto> ContributingStations { get; set; } = new();
        public DateTime EvaluatedAt { get; set; }
        public AdministrativeAreaInfoDto? AdministrativeArea { get; set; }
        public object? GeoJson { get; set; }
    }

    public class AdministrativeAreaInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string? Code { get; set; }
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
    }

    public class ContributingStationDto
    {
        public Guid StationId { get; set; }
        public string StationCode { get; set; } = string.Empty;
        public double Distance { get; set; }
        public double WaterLevel { get; set; }
        public string Severity { get; set; } = string.Empty;
        public int Weight { get; set; }
        
        // Ward information (for district and city level)
        public ContributingWardInfoDto? Ward { get; set; }
        
        // District information (for city level)
        public ContributingDistrictInfoDto? District { get; set; }
    }
    
    public class ContributingWardInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }
    
    public class ContributingDistrictInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }
}

