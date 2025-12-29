using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG13
{
    public class GoogleOAuthCallbackRequest : IFeatureRequest<GoogleOAuthCallbackResponse>
    {
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}
