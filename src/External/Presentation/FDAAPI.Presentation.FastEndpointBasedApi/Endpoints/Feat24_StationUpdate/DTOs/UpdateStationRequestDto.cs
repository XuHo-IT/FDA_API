using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat22_StationUpdate.DTOs
{
    public class UpdateStationRequestDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string LocationDesc { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string RoadName { get; set; }
        public string Direction { get; set; }
        public string Status { get; set; }
        public decimal? ThresholdWarning { get; set; }
        public decimal? ThresholdCritical { get; set; }
        public DateTimeOffset? InstalledAt { get; set; }
        public DateTimeOffset? LastSeenAt { get; set; }
    }
}

