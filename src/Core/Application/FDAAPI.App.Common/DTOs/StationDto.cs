using System;

namespace FDAAPI.App.Common.DTOs
{
    public class StationDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LocationDesc { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string RoadName { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? InstalledAt { get; set; }
        public DateTimeOffset? LastSeenAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

