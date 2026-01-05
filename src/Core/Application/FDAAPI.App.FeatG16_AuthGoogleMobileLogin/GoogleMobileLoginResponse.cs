using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG16_AuthGoogleMobileLogin
{
    /// <summary>
    /// Response from mobile Google OAuth login
    /// </summary>
    public class GoogleMobileLoginResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserDto? User { get; set; }
    }

    /// <summary>
    /// User information DTO
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}






