using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.FloodEvents;
using System.Collections.Generic;

namespace FDAAPI.App.FeatG63_FloodEventList
{
    public class GetFloodEventsResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FloodEventStatusCode StatusCode { get; set; }
        public List<FloodEventDto> FloodEvents { get; set; } = new();
        public int TotalCount { get; set; }
    }
}

