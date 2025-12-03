using FastEndpoints;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat2.DTOs
{
    /// <summary>
    /// Data Transfer Object for Update Water Level request
    /// </summary>
    public class UpdateWaterLevelRequestDto
    {
        [BindFrom("waterLevelId")]
        public long WaterLevelId { get; set; }
        public double NewWaterLevel { get; set; }
        public string LocationName { get; set; }
        public string Unit { get; set; } = "meters";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
