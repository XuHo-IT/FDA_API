using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG52_GetSeverityAnalytics
{
    public sealed record GetSeverityAnalyticsRequest(
        Guid? AdministrativeAreaId = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        string BucketType = "day"  // "day", "week", "month", "year"
    ) : IFeatureRequest<GetSeverityAnalyticsResponse>;
}

