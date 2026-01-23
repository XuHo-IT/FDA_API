using System;
using System.Collections.Generic;

namespace FDAAPI.App.Common.DTOs
{
    public class AdministrativeAreaStatusDto
    {
        public Guid AdministrativeAreaId { get; set; }
        public string Status { get; set; } = "Unknown";
        public int SeverityLevel { get; set; } = -1;
        public string Summary { get; set; } = string.Empty;
        public List<ContributingStationDto> ContributingStations { get; set; } = new();
        public DateTime EvaluatedAt { get; set; }
        
        // Administrative Area full information
        public AdministrativeAreaInfoDto? AdministrativeArea { get; set; }
        
        // GeoJSON from Geometry field
        public object? GeoJson { get; set; }
    }
    
    public class AdministrativeAreaInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty; // "ward", "district", "city"
        public string? Code { get; set; }
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
    }
}

