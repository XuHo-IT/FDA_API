using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG3_WaterLevelGet
{
    /// <summary>
    /// Response from getting a water level record
    /// </summary>
    public class GetWaterLevelResponse : IFeatureResponse
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






