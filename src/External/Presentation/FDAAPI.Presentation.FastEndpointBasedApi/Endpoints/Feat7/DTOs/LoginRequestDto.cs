namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat7.DTOs
{
    /// <summary>
    /// Data Transfer Object for Login request
    /// Supports two authentication methods:
    /// 1. Phone + OTP (for Citizens)
    /// 2. Email + Password (for Admin/Gov)
    /// </summary>
    public class LoginRequestDto
    {
        // Phone + OTP Login (Citizen)
        public string? PhoneNumber { get; set; }
        public string? OtpCode { get; set; }

        // Email + Password Login (Admin/Gov)
        public string? Email { get; set; }
        public string? Password { get; set; }

        // Device tracking (optional)
        public string? DeviceInfo { get; set; }
    }
}
