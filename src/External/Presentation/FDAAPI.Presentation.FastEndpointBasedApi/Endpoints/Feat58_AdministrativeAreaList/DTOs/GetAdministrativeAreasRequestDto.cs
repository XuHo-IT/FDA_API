using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat58_AdministrativeAreaList.DTOs
{
    public class GetAdministrativeAreasRequestDto
    {
        public string? SearchTerm { get; set; }
        public string? Level { get; set; } // "ward", "district", "city"
        public Guid? ParentId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

