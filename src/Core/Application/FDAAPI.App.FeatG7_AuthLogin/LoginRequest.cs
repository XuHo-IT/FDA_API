using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG7_AuthLogin
{
    /// <summary>
    /// Request for user login
    /// Supports two authentication methods:
    /// 1. Phone + OTP (for Citizens)
    /// 2. Email + Password (for Admin/Gov)
    /// </summary>
    public sealed record LoginRequest(string? Identifier,string? PhoneNumber, string? OtpCode,string? Email, string? Password, string? DeviceInfo, string? IpAddress) : IFeatureRequest<LoginResponse>;
}






