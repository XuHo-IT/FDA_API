using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG35_AreaGet
{
    public sealed record GetAreaRequest(Guid Id) : IFeatureRequest<GetAreaResponse>;
}

