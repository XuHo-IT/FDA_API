using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG10_AuthChangePassword
{
    public sealed record ChangePasswordRequest(Guid UserId, string CurrentPassword, string NewPassword, string ConfirmPassword) : IFeatureRequest<ChangePasswordResponse>;
}






