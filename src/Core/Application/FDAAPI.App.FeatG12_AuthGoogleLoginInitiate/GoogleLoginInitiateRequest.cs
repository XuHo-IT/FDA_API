using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG12_AuthGoogleLoginInitiate
{
    public sealed record GoogleLoginInitiateRequest(string? ReturnUrl) : IFeatureRequest<GoogleLoginInitiateResponse>;
}






