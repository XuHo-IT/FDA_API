using FDAAPI.App.Common.Features;

namespace FDAAPI.App.Feat4
{
    /// <summary>
    /// Request to delete a water level record
    /// </summary>
    public class DeleteWaterLevelRequest : IFeatureRequest<DeleteWaterLevelResponse>
    {
        public long WaterLevelId { get; set; }
    }
}
