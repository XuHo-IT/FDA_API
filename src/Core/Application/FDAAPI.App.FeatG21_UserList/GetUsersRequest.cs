using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG21_UserList
{
    public sealed record GetUsersRequest(
        string? SearchTerm,
        string? Role,
        string? Status,
        int PageNumber = 1,
        int PageSize = 10) : IFeatureRequest<GetUsersResponse>;
}

