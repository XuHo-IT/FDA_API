using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG78_GetPredictionAccuracyStats.DTOs
{
    public class GetPredictionAccuracyStatsRequestDto
    {
        public Guid? AreaId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string GroupBy { get; set; } = "day";
    }
}

