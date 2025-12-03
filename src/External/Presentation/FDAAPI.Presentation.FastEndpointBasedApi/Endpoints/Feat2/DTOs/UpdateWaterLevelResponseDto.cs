namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2.DTOs
{
    /// <summary>
    /// Data Transfer Object for Update Water Level response
    /// </summary>
    public class UpdateWaterLevelResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long WaterLevelId { get; set; }
        public double NewWaterLevel { get; set; }
        public string LocationName { get; set; } 
        public DateTime UpdatedAt { get; set; }
    }
}
