using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG51_GetFrequencyAnalytics
{
    public sealed record GetFrequencyAnalyticsRequest(
        Guid? AdministrativeAreaId = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        string BucketType = "day"  // "day", "week", "month", "year"
    ) : IFeatureRequest<GetFrequencyAnalyticsResponse>;
}

