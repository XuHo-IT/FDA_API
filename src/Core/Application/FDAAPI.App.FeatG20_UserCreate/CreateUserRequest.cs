using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG20_UserCreate
{
    public sealed record CreateUserRequest(
        Guid AdminId,
        string Email,
        string Password,
        string FullName,
        string? PhoneNumber,
        List<string> RoleNames) : IFeatureRequest<CreateUserResponse>;
}

