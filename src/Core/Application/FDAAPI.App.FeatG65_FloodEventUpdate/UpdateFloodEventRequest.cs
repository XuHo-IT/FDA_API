using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG65_FloodEventUpdate
{
    public record UpdateFloodEventRequest(
        Guid Id,
        Guid AdministrativeAreaId,
        DateTime StartTime,
        DateTime EndTime,
        decimal? PeakLevel,
        int? DurationHours,
        Guid AdminId
    ) : IFeatureRequest<UpdateFloodEventResponse>;
}

