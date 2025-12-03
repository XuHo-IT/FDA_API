using FDAAPI.App.Common.Features;

namespace FDAAPI.App.Feat2
{
    /// <summary>
    /// Request to update a water level record
    /// </summary>
    public class UpdateWaterLevelRequest : IFeatureRequest<UpdateWaterLevelResponse>
    {
        public long WaterLevelId { get; set; }
        public double NewWaterLevel { get; set; }
        public string LocationName { get; set; } 
        public string Unit { get; set; } = "meters";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
