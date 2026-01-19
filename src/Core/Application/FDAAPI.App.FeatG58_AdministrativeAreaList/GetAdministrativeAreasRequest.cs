using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG58_AdministrativeAreaList
{
    public sealed record GetAdministrativeAreasRequest(
        string? SearchTerm,
        string? Level, // "ward", "district", "city"
        Guid? ParentId,
        int PageNumber = 1,
        int PageSize = 10) : IFeatureRequest<GetAdministrativeAreasResponse>;
}

