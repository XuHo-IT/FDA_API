using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG20_AdminManagement.Features.Users.Update
{
    public sealed record UpdateUserRequest(
        Guid AdminId,
        Guid UserId,
        string? FullName = null,
        string? PhoneNumber = null,
        string? Status = null,
        List<string>? RoleNames = null) : IFeatureRequest<UpdateUserResponse>;
}







