using FDAAPI.App.Common.Features;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat14.DTOs
{
    /// <summary>
    /// DTO for mobile Google OAuth login response
    /// </summary>
    public class GoogleMobileLoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = new();
    }

}
