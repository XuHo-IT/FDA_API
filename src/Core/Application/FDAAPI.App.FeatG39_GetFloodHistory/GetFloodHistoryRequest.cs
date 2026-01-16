using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG39_GetFloodHistory
{
    public sealed record GetFloodHistoryRequest(
        Guid? StationId,
        List<Guid>? StationIds,
        Guid? AreaId,
        DateTime? StartDate,
        DateTime? EndDate,
        string Granularity = "hourly",  // raw, hourly, daily
        int Limit = 1000,
        string? Cursor = null
    ) : IFeatureRequest<GetFloodHistoryResponse>;
}
