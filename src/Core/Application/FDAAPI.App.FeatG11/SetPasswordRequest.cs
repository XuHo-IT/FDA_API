using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG11
{
    public sealed record SetPasswordRequest(Guid UserId, string Email, string NewPassword, string ConfirmPassword) : IFeatureRequest<SetPasswordResponse>;
}
