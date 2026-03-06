using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG107_StationComponentDelete
{
    public class DeleteStationComponentRequest : IFeatureRequest<StationComponentResponse>
    {
        public Guid Id { get; set; }
    }
}
