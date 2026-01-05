using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG6_AuthSendOtp
{
    /// <summary>
    /// Request to send OTP to phone number
    /// </summary>
    public sealed record SendOtpRequest(string Identifier) : IFeatureRequest<SendOtpResponse>;

}






