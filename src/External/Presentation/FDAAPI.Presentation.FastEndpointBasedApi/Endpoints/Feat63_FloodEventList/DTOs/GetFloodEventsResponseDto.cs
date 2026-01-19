using FDAAPI.App.Common.DTOs;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat63_FloodEventList.DTOs
{
    public class GetFloodEventsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public List<FloodEventDto> FloodEvents { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

