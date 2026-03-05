using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;

namespace FDAAPI.App.FeatG48_SeverityAggregation
{
    public sealed record SeverityAggregationRequest(
        string BucketType,  // "day", "week", "month", "year"
        DateTime StartDate,
        DateTime EndDate,
        List<Guid>? AdministrativeAreaIds = null  // null = all areas
    ) : IFeatureRequest<SeverityAggregationResponse>;
}

