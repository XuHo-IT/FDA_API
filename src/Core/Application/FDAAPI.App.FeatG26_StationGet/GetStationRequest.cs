using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG24_StationGet
{
    public sealed record GetStationRequest(Guid Id) : IFeatureRequest<GetStationResponse>;
}

