using FDAAPI.App.Common.Features;
using MediatR;

namespace FDAAPI.App.FeatG25_StationList
{
    public sealed record GetStationsRequest(
        string? SearchTerm,
        string? Status,
        int PageNumber = 1,
        int PageSize = 10) : IFeatureRequest<GetStationsResponse>;
}

