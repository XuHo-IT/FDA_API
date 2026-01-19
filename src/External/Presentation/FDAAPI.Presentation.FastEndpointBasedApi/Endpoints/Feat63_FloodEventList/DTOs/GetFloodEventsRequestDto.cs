using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat63_FloodEventList.DTOs
{
    public class GetFloodEventsRequestDto
    {
        public string? SearchTerm { get; set; }
        public Guid? AdministrativeAreaId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

