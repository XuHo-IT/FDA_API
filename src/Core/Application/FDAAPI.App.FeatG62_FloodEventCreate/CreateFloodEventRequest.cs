using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG62_FloodEventCreate
{
    public record CreateFloodEventRequest(
        Guid AdministrativeAreaId,
        DateTime StartTime,
        DateTime EndTime,
        decimal? PeakLevel,
        int? DurationHours,
        Guid AdminId
    ) : IFeatureRequest<CreateFloodEventResponse>;
}

