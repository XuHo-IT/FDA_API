namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat46_GetFloodStatistics.DTOs
{
    /// <summary>
    /// Request DTO for getting flood statistics
    /// </summary>
    public class GetFloodStatisticsRequestDto
    {
        /// <summary>
        /// Filter by single station ID (optional)
        /// </summary>
        public Guid? StationId { get; set; }

        /// <summary>
        /// Filter by multiple station IDs (optional)
        /// </summary>
        public List<Guid>? StationIds { get; set; }

        /// <summary>
        /// Filter by user's monitored area ID (optional)
        /// </summary>
        public Guid? AreaId { get; set; }

        /// <summary>
        /// Time period: last7days, last30days, last90days, last365days
        /// Default: last30days
        /// </summary>
        public string Period { get; set; } = "last30days";

        /// <summary>
        /// Include severity breakdown (hours per severity level)
        /// Default: true
        /// </summary>
        public bool IncludeBreakdown { get; set; } = true;

        /// <summary>
        /// Include comparison with previous period
        /// Default: false
        /// </summary>
        public bool IncludeComparison { get; set; } = false;
    }
}
