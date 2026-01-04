using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG6
{
    /// <summary>
    /// Request to send OTP to phone number
    /// </summary>
    public sealed record SendOtpRequest(string Identifier) : IFeatureRequest<SendOtpResponse>;

}
