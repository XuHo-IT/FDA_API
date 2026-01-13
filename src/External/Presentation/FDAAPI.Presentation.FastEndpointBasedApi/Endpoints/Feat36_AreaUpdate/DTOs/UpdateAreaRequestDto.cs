using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat36_AreaUpdate.DTOs
{
    public class UpdateAreaRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusMeters { get; set; }
        public string AddressText { get; set; } = string.Empty;
    }
}

