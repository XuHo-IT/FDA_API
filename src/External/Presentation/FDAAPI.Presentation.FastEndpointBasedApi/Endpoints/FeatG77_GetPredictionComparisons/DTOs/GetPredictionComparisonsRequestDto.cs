using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.FeatG77_GetPredictionComparisons.DTOs
{
    public class GetPredictionComparisonsRequestDto
    {
        public Guid? AreaId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsVerified { get; set; }
        public decimal? MinAccuracy { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 50;
    }
}

