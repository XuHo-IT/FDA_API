using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG37_AreaDelete
{
    public sealed record DeleteAreaRequest(Guid Id, Guid UserId) : IFeatureRequest<DeleteAreaResponse>;
}

