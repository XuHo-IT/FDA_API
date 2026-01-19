using System;

namespace FDAAPI.App.Common.DTOs
{
    public class FloodEventDto
    {
        public Guid Id { get; set; }
        public Guid AdministrativeAreaId { get; set; }
        public string? AdministrativeAreaName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal? PeakLevel { get; set; }
        public int? DurationHours { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

