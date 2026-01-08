using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG27_StationDelete
{
    public sealed record DeleteStationRequest(Guid Id) : IFeatureRequest<DeleteStationResponse>;
}

