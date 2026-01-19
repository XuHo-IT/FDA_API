using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat51_GetFrequencyAnalytics.DTOs
{
    public class GetFrequencyAnalyticsRequestDto
    {
        public Guid? AdministrativeAreaId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string BucketType { get; set; } = "day";
    }
}

