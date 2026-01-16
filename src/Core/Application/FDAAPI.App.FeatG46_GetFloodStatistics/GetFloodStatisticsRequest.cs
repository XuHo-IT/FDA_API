using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG46_GetFloodStatistics
{
    public sealed record GetFloodStatisticsRequest(
        Guid? StationId,
        List<Guid>? StationIds,
        Guid? AreaId,
        string Period = "last30days",
        bool IncludeBreakdown = true,
        bool IncludeComparison = false
    ) : IFeatureRequest<GetFloodStatisticsResponse>;
}
