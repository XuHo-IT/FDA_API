using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat21_StationCreate.DTOs
{
    public class CreateStationRequestDto
    {
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
        public Guid? AdministrativeAreaId { get; set; }
        public DateTimeOffset? InstalledAt { get; set; }
    }
}

