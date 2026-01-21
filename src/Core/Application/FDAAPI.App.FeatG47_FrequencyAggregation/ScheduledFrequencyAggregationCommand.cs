using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Analytics;

namespace FDAAPI.App.FeatG47_FrequencyAggregation
{
    public sealed record ScheduledFrequencyAggregationCommand(
        AggregationMode Mode) : IFeatureRequest<UnitResponse>;
}

