using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat60_AdministrativeAreaUpdate.DTOs
{
    public class UpdateAdministrativeAreaRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty; // "ward", "district", "city"
        public Guid? ParentId { get; set; }
        public string? Code { get; set; }
        public string? Geometry { get; set; } // JSON string for PostGIS geometry
    }
}

