using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG12
{
    public sealed record GoogleLoginInitiateRequest(string? ReturnUrl) : IFeatureRequest<GoogleLoginInitiateResponse>;
}
