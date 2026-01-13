using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat32_AreaCreate.DTOs
{
    public class CreateAreaRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusMeters { get; set; } = 1000;
        public string AddressText { get; set; } = string.Empty;
    }
}

