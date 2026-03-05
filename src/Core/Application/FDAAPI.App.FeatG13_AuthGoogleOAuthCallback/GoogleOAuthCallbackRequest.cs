using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG13_AuthGoogleOAuthCallback
{
    public sealed record GoogleOAuthCallbackRequest(string Code, string State) : IFeatureRequest<GoogleOAuthCallbackResponse>;
}






