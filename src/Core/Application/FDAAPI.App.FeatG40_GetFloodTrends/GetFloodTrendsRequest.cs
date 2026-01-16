using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG40_GetFloodTrends
{
    public sealed record GetFloodTrendsRequest(
        Guid StationId,
        string Period = "last30days",  // last7days, last30days, last90days, last365days, custom
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        string Granularity = "daily",  // daily, weekly, monthly
        bool CompareWithPrevious = false
    ) : IFeatureRequest<GetFloodTrendsResponse>;
}
