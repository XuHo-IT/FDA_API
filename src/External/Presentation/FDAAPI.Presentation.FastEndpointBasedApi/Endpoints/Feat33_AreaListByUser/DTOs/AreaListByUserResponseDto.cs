using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat33_AreaListByUser.DTOs
{
    public class AreaListByUserResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public List<AreaDto> Areas { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class AreaDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RadiusMeters { get; set; }
        public string AddressText { get; set; } = string.Empty;
    }
}
