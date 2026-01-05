using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG11_AuthSetPassword
{
    public sealed record SetPasswordRequest(Guid UserId, string Email, string NewPassword, string ConfirmPassword) : IFeatureRequest<SetPasswordResponse>;
}






