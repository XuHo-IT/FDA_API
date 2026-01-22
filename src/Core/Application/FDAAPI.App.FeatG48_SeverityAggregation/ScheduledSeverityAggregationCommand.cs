using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Analytics;

namespace FDAAPI.App.FeatG48_SeverityAggregation
{
    public sealed record ScheduledSeverityAggregationCommand(
        AggregationMode Mode) : IFeatureRequest<UnitResponse>;
}

