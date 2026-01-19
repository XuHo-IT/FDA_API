using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG49_HotspotAggregation
{
    public sealed record HotspotAggregationRequest(
        DateTime PeriodStart,
        DateTime PeriodEnd,
        int? TopN = null  // null = all hotspots
    ) : IFeatureRequest<HotspotAggregationResponse>;
}

