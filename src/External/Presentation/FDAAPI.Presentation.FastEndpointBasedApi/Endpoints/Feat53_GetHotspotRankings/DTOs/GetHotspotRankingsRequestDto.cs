using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat53_GetHotspotRankings.DTOs
{
    public class GetHotspotRankingsRequestDto
    {
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public int? TopN { get; set; } = 20;
        public string? AreaLevel { get; set; } = "district";
    }
}

