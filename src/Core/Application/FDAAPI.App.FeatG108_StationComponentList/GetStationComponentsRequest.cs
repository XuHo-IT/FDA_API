using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG108_StationComponentList
{
    public class GetStationComponentsRequest : IFeatureRequest<StationComponentListResponse>
    {
        public Guid StationId { get; set; }
    }
}
