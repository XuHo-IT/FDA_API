namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat44_GetFloodHistory.DTOs
{
    /// <summary>
    /// Request DTO for getting flood history data with time-range filters
    /// </summary>
    public class GetFloodHistoryRequestDto
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
        /// Will fetch all stations within the area's radius
        /// </summary>
        public Guid? AreaId { get; set; }

        /// <summary>
        /// Start of time range (default: 24 hours ago)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End of time range (default: now)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Data granularity: raw, hourly, daily
        /// Default: raw
        /// </summary>
        public string Granularity { get; set; } = "hourly";

        /// <summary>
        /// Maximum number of data points per station
        /// Default: 1000
        /// </summary>
        public int Limit { get; set; } = 1000;

        /// <summary>
        /// Pagination cursor for next page (optional)
        /// </summary>
        public string? Cursor { get; set; }
    }
}
