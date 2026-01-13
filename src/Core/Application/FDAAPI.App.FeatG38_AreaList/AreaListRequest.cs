using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG38_AreaList
{
    public sealed record AreaListRequest(
        string? SearchTerm = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IFeatureRequest<AreaListResponse>;
}

