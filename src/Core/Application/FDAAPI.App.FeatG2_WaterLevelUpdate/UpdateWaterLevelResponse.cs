using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG2_WaterLevelUpdate
{
    /// <summary>
    /// Response from updating a water level record
    /// </summary>
    public class UpdateWaterLevelResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long WaterLevelId { get; set; }
        public string LocationName { get; set; }
        public double NewWaterLevel { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}






