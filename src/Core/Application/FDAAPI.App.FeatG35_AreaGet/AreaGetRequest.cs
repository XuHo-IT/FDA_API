using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG35_AreaGet
{
    public sealed record AreaGetRequest(Guid Id) : IFeatureRequest<AreaGetResponse>;
}

