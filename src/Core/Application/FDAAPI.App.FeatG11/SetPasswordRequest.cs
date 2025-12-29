using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG11
{
    public class SetPasswordRequest : IFeatureRequest<SetPasswordResponse>
    {
        public Guid UserId { get; set; }  // From JWT claims
        public string Email { get; set; } = string.Empty;  // Optional: update email for email-based login
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
