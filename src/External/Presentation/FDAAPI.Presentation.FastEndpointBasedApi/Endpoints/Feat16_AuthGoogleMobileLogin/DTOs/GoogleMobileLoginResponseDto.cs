using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat16_AuthGoogleMobileLogin.DTOs{
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
}








