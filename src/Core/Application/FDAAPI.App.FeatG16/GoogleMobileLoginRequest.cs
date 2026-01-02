using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG16
{
    /// <summary>
    /// Request to login via Google OAuth from mobile app
    /// Mobile SDK (React Native) provides the idToken directly
    /// </summary>
    public class GoogleMobileLoginRequest : IFeatureRequest<GoogleMobileLoginResponse>
    {
        /// <summary>
        /// ID Token from Google Sign-In SDK (React Native)
        /// </summary>
        public string IdToken { get; set; } = string.Empty;
    }
}

