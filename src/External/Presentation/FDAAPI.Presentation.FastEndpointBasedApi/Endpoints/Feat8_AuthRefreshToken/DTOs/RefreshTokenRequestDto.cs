namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat8_AuthRefreshToken.DTOs{
    /// <summary>
    /// Data Transfer Object for Refresh Token request
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// Current refresh token to exchange for new tokens
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}








