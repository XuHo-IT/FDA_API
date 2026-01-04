using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG8
{
    /// <summary>
    /// Request to refresh access token using refresh token
    /// </summary>
    public sealed record RefreshTokenRequest(string RefreshToken) : IFeatureRequest<RefreshTokenResponse>;
}
