using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat52_GetSeverityAnalytics.DTOs
{
    public class GetSeverityAnalyticsRequestDto
    {
        public Guid? AdministrativeAreaId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string BucketType { get; set; } = "day";
    }
}

