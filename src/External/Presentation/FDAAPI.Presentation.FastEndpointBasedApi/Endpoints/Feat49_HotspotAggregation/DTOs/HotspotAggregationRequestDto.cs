using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat49_HotspotAggregation.DTOs
{
    public class HotspotAggregationRequestDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int? TopN { get; set; }
    }
}

