using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG5_AuthResetPassword
{
    public sealed record ResetPasswordRequest(
        Guid UserId,
        string NewPassword,
        string ConfirmPassword
    ) : IFeatureRequest<ResetPasswordResponse>;
}
