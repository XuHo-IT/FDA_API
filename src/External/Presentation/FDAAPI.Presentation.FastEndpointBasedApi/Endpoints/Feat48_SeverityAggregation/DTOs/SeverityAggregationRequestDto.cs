using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat48_SeverityAggregation.DTOs
{
    public class SeverityAggregationRequestDto
    {
        public string BucketType { get; set; } = "day";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Guid>? AdministrativeAreaIds { get; set; }
    }
}

