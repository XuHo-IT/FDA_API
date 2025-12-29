namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat6.DTOs
{
    /// <summary>
    /// Data Transfer Object for Send OTP response
    /// </summary>
    public class SendOtpResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// OTP code (for development/testing only - remove in production)
        /// </summary>
        public string? OtpCode { get; set; }

        /// <summary>
        /// OTP expiration timestamp
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
