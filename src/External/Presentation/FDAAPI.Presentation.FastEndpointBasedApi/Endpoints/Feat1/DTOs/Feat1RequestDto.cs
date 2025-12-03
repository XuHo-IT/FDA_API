namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat1.DTOs
{
    /// <summary>
    /// Data Transfer Object for Create Water Level request
    /// </summary>
    public class CreateWaterLevelRequestDto
    {
        public string LocationName { get; set; } = string.Empty;
        public double WaterLevel { get; set; }
        public string Unit { get; set; } = "meters";
        public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
    }
}
