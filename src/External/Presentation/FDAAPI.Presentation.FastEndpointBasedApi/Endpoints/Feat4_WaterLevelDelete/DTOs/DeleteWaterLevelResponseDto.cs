namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat4_WaterLevelDelete.DTOs{
    /// <summary>
    /// Data Transfer Object for Delete Water Level response
    /// </summary>
    public class DeleteWaterLevelResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long WaterLevelId { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}








