using FDAAPI.App.Common.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG33_AreaList
{
    public sealed record AreaListRequest(
        Guid UserId,
        string? SearchTerm = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IFeatureRequest<AreaListResponse>;
}

