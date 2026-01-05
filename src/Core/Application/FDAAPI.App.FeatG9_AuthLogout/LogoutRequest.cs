using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG9_AuthLogout
{
    /// <summary>
    /// Request to logout user
    /// </summary>
    public sealed record LogoutRequest(string RefreshToken, bool RevokeAllTokens = false) : IFeatureRequest<LogoutResponse>;
}






