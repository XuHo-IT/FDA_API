namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat40_GetFloodTrends.DTOs
{
    /// <summary>
    /// Request DTO for getting flood trends data
    /// </summary>
    public class GetFloodTrendsRequestDto
    {
        /// <summary>
        /// Station ID to get trends for (required)
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// Time period: last7days, last30days, last90days, last365days, custom
        /// Default: last30days
        /// </summary>
        public string Period { get; set; } = "last30days";

        /// <summary>
        /// Custom start date (only used when Period = "custom")
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Custom end date (only used when Period = "custom")
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Data granularity: daily, weekly, monthly
        /// Default: daily
        /// </summary>
        public string Granularity { get; set; } = "daily";

        /// <summary>
        /// Include comparison with previous period
        /// Default: false
        /// </summary>
        public bool CompareWithPrevious { get; set; } = false;
    }
}
