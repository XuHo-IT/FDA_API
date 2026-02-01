using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG77_GetPredictionComparisons
{
    public sealed record GetPredictionComparisonsRequest(
        Guid? AreaId,
        DateTime? StartDate,
        DateTime? EndDate,
        bool? IsVerified,
        decimal? MinAccuracy,
        int Page = 1,
        int Size = 50
    ) : IFeatureRequest<GetPredictionComparisonsResponse>;
}

