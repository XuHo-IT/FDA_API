using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG63_FloodEventList
{
    public sealed record GetFloodEventsRequest(
        string? SearchTerm,
        Guid? AdministrativeAreaId,
        DateTime? StartDate,
        DateTime? EndDate,
        int PageNumber = 1,
        int PageSize = 10) : IFeatureRequest<GetFloodEventsResponse>;
}

