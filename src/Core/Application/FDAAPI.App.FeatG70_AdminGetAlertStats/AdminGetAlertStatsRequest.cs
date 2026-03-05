using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG70_AdminGetAlertStats
{
    public sealed record AdminGetAlertStatsRequest(
        DateTime? FromDate = null,
        DateTime? ToDate = null
    ) : IFeatureRequest<AdminGetAlertStatsResponse>;
}