namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat3.DTOs
{
    /// <summary>
    /// Data Transfer Object for Get Water Level response
    /// </summary>
    public class GetWaterLevelResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long WaterLevelId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public double WaterLevel { get; set; }
        public string Unit { get; set; } = "meters";
        public DateTime MeasuredAt { get; set; }
    }
}
