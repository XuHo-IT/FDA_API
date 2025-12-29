namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat9.DTOs
{
    /// <summary>
    /// Data Transfer Object for Logout request
    /// </summary>
    public class LogoutRequestDto
    {
        /// <summary>
        /// Refresh token to revoke
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// If true, logout from ALL devices (revoke all user tokens)
        /// If false, logout from current device only
        /// </summary>
        public bool RevokeAllTokens { get; set; } = false;
    }
}
