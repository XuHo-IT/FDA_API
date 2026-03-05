using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG57_AdministrativeAreaCreate
{
    public record CreateAdministrativeAreaRequest(
        string Name,
        string Level, // "ward", "district", "city"
        Guid? ParentId,
        string? Code,
        string? Geometry, // JSON string for PostGIS geometry
        Guid AdminId
    ) : IFeatureRequest<CreateAdministrativeAreaResponse>;
}

