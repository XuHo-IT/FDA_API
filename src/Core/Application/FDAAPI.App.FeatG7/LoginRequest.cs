using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG7
{
    /// <summary>
    /// Request for user login
    /// Supports two authentication methods:
    /// 1. Phone + OTP (for Citizens)
    /// 2. Email + Password (for Admin/Gov)
    /// </summary>
    public class LoginRequest : IFeatureRequest<LoginResponse>
    {
        // Unified identifier (can be phone or email)
        public string? Identifier { get; set; }

        // For Phone + OTP Login
        public string? PhoneNumber { get; set; }
        public string? OtpCode { get; set; }

        // For Email + Password Login
        public string? Email { get; set; }
        public string? Password { get; set; }

        // Device tracking (optional)
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
    }
}
