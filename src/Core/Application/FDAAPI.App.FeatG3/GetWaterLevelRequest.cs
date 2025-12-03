using FDAAPI.App.Common.Features;

namespace FDAAPI.App.Feat3
{
    /// <summary>
    /// Request to get/retrieve a water level record
    /// </summary>
    public class GetWaterLevelRequest : IFeatureRequest<GetWaterLevelResponse>
    {
        public long WaterLevelId { get; set; }
    }
}
