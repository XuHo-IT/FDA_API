using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG4_WaterLevelDelete
{
    /// <summary>
    /// Request to delete a water level record
    /// </summary>
    public class DeleteWaterLevelRequest : IFeatureRequest<DeleteWaterLevelResponse>
    {
        public long WaterLevelId { get; set; }
    }
}






