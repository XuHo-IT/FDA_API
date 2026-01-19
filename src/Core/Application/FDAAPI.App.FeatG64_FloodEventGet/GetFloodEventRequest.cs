using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG64_FloodEventGet
{
    public record GetFloodEventRequest(
        Guid Id
    ) : IFeatureRequest<GetFloodEventResponse>;
}

