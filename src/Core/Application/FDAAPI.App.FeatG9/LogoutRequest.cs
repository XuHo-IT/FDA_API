using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG9
{
    /// <summary>
    /// Request to logout user
    /// </summary>
    public class LogoutRequest : IFeatureRequest<LogoutResponse>
    {
        /// <summary>
        /// Refresh token to revoke
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// If true, revoke ALL refresh tokens for this user (logout from all devices)
        /// If false, only revoke the specific refresh token (logout from current device)
        /// Default: false
        /// </summary>
        public bool RevokeAllTokens { get; set; } = false;
    }
}
