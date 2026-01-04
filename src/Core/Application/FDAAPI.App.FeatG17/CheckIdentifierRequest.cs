using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG17
{
    /// <summary>
    /// Request to check if an identifier (phone/email) exists and requires password or OTP
    /// </summary>
    public sealed record CheckIdentifierRequest(string Identifier) : IFeatureRequest<CheckIdentifierResponse>;

}
