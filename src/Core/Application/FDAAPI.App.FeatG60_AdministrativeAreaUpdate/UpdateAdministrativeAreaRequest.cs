using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG60_AdministrativeAreaUpdate
{
    public record UpdateAdministrativeAreaRequest(
        Guid Id,
        string Name,
        string Level, // "ward", "district", "city"
        Guid? ParentId,
        string? Code,
        string? Geometry, // JSON string for PostGIS geometry
        Guid AdminId
    ) : IFeatureRequest<UpdateAdministrativeAreaResponse>;
}

