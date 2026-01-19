using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat53_GetHotspotRankings.DTOs
{
    public class GetHotspotRankingsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public HotspotRankingsDto? Data { get; set; }
    }

    public class HotspotRankingsDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string AreaLevel { get; set; } = string.Empty;
        public List<HotspotDto> Hotspots { get; set; } = new();
    }

    public class HotspotDto
    {
        public Guid AdministrativeAreaId { get; set; }
        public string AdministrativeAreaName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int Rank { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}

