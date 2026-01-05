using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG3_WaterLevelGet
{
    /// <summary>
    /// Request to get/retrieve a water level record
    /// </summary>
    public class GetWaterLevelRequest : IFeatureRequest<GetWaterLevelResponse>
    {
        public long WaterLevelId { get; set; }
    }
}






