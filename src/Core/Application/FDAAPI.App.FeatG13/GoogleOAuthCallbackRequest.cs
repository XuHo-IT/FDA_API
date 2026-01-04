using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG13
{
    public sealed record GoogleOAuthCallbackRequest(string Code, string State) : IFeatureRequest<GoogleOAuthCallbackResponse>;
}
