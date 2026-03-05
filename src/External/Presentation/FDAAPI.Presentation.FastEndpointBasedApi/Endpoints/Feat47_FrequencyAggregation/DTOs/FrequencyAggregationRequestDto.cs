using System;
using System.Collections.Generic;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat47_FrequencyAggregation.DTOs
{
    public class FrequencyAggregationRequestDto
    {
        public string BucketType { get; set; } = "day";  // "day", "week", "month", "year"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Guid>? AdministrativeAreaIds { get; set; }  // null = all areas
    }
}

