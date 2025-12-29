using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG8
{
    /// <summary>
    /// Request to refresh access token using refresh token
    /// </summary>
    public class RefreshTokenRequest : IFeatureRequest<RefreshTokenResponse>
    {
        /// <summary>
        /// Current refresh token (Base64-encoded)
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
