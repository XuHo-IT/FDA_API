using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG36_AreaUpdate
{
    public sealed record UpdateAreaRequest(
        Guid Id,
        Guid UserId,
        string Name,
        decimal Latitude,
        decimal Longitude,
        int RadiusMeters,
        string AddressText
    ) : IFeatureRequest<UpdateAreaResponse>;
}

