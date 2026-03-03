using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG78_GetPredictionAccuracyStats
{
    public sealed record GetPredictionAccuracyStatsRequest(
        Guid? AreaId,
        DateTime? StartDate,
        DateTime? EndDate,
        string GroupBy = "day"  // day, week, month
    ) : IFeatureRequest<GetPredictionAccuracyStatsResponse>;
}

