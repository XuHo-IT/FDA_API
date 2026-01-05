namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat7_AuthLogin.DTOs{
    /// <summary>
    /// Data Transfer Object for Login request
    /// Supports 4 authentication methods:
    /// 1. Phone + OTP (for registration/login)
    /// 2. Phone + Password (for login with password)
    /// 3. Email + OTP (for registration/login/forgot password)
    /// 4. Email + Password (for login with password)
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Phone number or email address
        /// </summary>
        public string? Identifier { get; set; }

        /// <summary>
        /// OTP code (for OTP-based authentication)
        /// </summary>
        public string? OtpCode { get; set; }

        /// <summary>
        /// Password (for password-based authentication)
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Device information (optional, for tracking)
        /// </summary>
        public string? DeviceInfo { get; set; }
    }
}







