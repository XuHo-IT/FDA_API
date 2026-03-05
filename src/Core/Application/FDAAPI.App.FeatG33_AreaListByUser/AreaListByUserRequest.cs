using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG33_AreaListByUser
{
    public sealed record AreaListByUserRequest(
        Guid UserId,
        string? SearchTerm = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IFeatureRequest<AreaListByUserResponse>;
}
