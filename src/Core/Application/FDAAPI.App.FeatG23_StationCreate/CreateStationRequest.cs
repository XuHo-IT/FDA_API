using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG23_StationCreate
{
    public record CreateStationRequest(
        string Code,
        string Name,
        string LocationDesc,
        decimal? Latitude,
        decimal? Longitude,
        string RoadName,
        string Direction,
        string Status,
        DateTimeOffset? InstalledAt,
        Guid AdminId
    ) : IFeatureRequest<CreateStationResponse>;
}

