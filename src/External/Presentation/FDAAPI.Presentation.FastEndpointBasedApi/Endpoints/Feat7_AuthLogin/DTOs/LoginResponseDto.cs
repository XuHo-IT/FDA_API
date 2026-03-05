using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat7_AuthLogin.DTOs{
    /// <summary>
    /// Data Transfer Object for Login response
    /// </summary>
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// JWT access token (60 minutes expiry)
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Refresh token (7 days expiry)
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Access token expiration timestamp
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// User information (no sensitive data)
        /// </summary>
        public UserDto? User { get; set; }
    }
}








