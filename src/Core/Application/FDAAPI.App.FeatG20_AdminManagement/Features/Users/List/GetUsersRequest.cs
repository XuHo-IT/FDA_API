using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG20_AdminManagement.Features.Users.List
{
    public sealed record GetUsersRequest(
        string? SearchTerm,
        string? Role,
        string? Status,
        int PageNumber = 1,
        int PageSize = 10) : IFeatureRequest<GetUsersResponse>;
}







