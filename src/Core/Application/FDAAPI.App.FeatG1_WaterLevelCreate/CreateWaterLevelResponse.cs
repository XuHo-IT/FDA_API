using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG1_WaterLevelCreate
{
    /// <summary>
    /// Response from creating a water level record
    /// </summary>
    public class CreateWaterLevelResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long WaterLevelId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public double WaterLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}






