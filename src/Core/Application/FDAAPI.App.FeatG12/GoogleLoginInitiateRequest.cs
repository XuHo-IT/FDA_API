using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG12
{
    public class GoogleLoginInitiateRequest : IFeatureRequest<GoogleLoginInitiateResponse>
    {
        public string? ReturnUrl { get; set; }
    }
}
