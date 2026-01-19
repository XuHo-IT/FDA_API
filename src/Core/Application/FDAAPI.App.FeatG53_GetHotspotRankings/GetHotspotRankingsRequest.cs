using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG53_GetHotspotRankings
{
    public sealed record GetHotspotRankingsRequest(
        DateTime? PeriodStart = null,
        DateTime? PeriodEnd = null,
        int? TopN = 20,  // Top N hotspots
        string? AreaLevel = "district"  // "ward" or "district"
    ) : IFeatureRequest<GetHotspotRankingsResponse>;
}

