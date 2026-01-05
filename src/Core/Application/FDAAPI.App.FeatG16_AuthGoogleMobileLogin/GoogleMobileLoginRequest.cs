using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG16_AuthGoogleMobileLogin
{
    /// <summary>
    /// Request to login via Google OAuth from mobile app
    /// Mobile SDK (React Native) provides the idToken directly
    /// </summary>
    public sealed record GoogleMobileLoginRequest(string IdToken) : IFeatureRequest<GoogleMobileLoginResponse>;

}







