namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat1.DTOs
{
    /// <summary>
    /// Data Transfer Object for Create Water Level response
    /// </summary>
    public class CreateWaterLevelResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long WaterLevelId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public double WaterLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
