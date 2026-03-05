using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG24_StationUpdate
{
    public record UpdateStationRequest(
        Guid Id,
        string Code,
        string Name,
        string LocationDesc,
        decimal? Latitude,
        decimal? Longitude,
        string RoadName,
        string Direction,
        string Status,
        decimal? ThresholdWarning,
        decimal? ThresholdCritical,
        Guid? AdministrativeAreaId,
        DateTimeOffset? InstalledAt,
        DateTimeOffset? LastSeenAt,
        Guid AdminId
    ) : IFeatureRequest<UpdateStationResponse>;
}

