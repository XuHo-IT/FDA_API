using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG66_FloodEventDelete
{
    public record DeleteFloodEventRequest(
        Guid Id
    ) : IFeatureRequest<DeleteFloodEventResponse>;
}

