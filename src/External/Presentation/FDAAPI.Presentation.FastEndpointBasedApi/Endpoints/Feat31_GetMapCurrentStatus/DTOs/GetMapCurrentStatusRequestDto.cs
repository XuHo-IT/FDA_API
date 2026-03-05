namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat31_GetMapCurrentStatus.DTOs
{
    public class GetMapCurrentStatusRequestDto
    {
        /// <summary>
        /// Minimum latitude for bounding box filter (optional)
        /// </summary>
        public decimal? MinLat { get; set; }

        /// <summary>
        /// Maximum latitude for bounding box filter (optional)
        /// </summary>
        public decimal? MaxLat { get; set; }

        /// <summary>
        /// Minimum longitude for bounding box filter (optional)
        /// </summary>
        public decimal? MinLng { get; set; }

        /// <summary>
        /// Maximum longitude for bounding box filter (optional)
        /// </summary>
        public decimal? MaxLng { get; set; }

        /// <summary>
        /// Filter by station status (active, offline, maintenance)
        /// Default: active
        /// </summary>
        public string? Status { get; set; } = "active";
    }
}