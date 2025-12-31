using FDAAPI.App.Common.Features;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat16.DTOs
{
    /// <summary>
    /// DTO for mobile Google OAuth login request
    /// </summary>
    public class GoogleMobileLoginRequestDto
    {
        /// <summary>
        /// ID Token from Google Sign-In SDK (React Native)
        /// </summary>
        public string IdToken { get; set; } = string.Empty;
    }

}
