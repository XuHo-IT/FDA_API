using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG109_StationComponentGet
{
    public class GetStationComponentByIdRequest : IFeatureRequest<StationComponentResponse>
    {
        public Guid Id { get; set; }
    }
}
