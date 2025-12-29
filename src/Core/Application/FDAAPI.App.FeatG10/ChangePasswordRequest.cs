using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG10
{
    public class ChangePasswordRequest : IFeatureRequest<ChangePasswordResponse>
    {
        public Guid UserId { get; set; }  // From JWT claims
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
