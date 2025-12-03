using FDAAPI.App.Common.Features;

namespace FDAAPI.App.Feat1
{
    /// <summary>
    /// Request to create a water level record
    /// </summary>
    public class CreateWaterLevelRequest : IFeatureRequest<CreateWaterLevelResponse>
    {
        public string LocationName { get; set; } = string.Empty;
        public double WaterLevel { get; set; }
        public string Unit { get; set; } = "meters";
        public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
    }
}
