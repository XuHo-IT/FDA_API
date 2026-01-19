using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat65_FloodEventUpdate.DTOs
{
    public class UpdateFloodEventRequestDto
    {
        public Guid AdministrativeAreaId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal? PeakLevel { get; set; }
        public int? DurationHours { get; set; }
    }
}

