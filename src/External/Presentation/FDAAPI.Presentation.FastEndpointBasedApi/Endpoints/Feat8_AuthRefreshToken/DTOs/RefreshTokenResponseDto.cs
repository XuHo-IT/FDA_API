namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat8_AuthRefreshToken.DTOs{
    /// <summary>
    /// Data Transfer Object for Refresh Token response
    /// </summary>
    public class RefreshTokenResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// New JWT access token
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// New refresh token (old token is automatically revoked)
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// New access token expiration
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}








