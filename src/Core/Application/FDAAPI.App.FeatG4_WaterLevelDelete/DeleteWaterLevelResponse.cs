using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG4_WaterLevelDelete
{
    /// <summary>
    /// Response from deleting a water level record
    /// </summary>
    public class DeleteWaterLevelResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long WaterLevelId { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}






