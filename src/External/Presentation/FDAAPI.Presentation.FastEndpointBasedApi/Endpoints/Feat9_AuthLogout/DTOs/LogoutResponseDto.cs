namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat9_AuthLogout.DTOs{
    /// <summary>
    /// Data Transfer Object for Logout response
    /// </summary>
    public class LogoutResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Number of tokens revoked
        /// </summary>
        public int TokensRevoked { get; set; }
    }
}








