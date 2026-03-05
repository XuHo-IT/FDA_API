using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Analytics;

namespace FDAAPI.App.FeatG49_HotspotAggregation
{
    public sealed record ScheduledHotspotAggregationCommand(
        AggregationMode Mode) : IFeatureRequest<UnitResponse>;
}

