using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG6
{
    /// <summary>
    /// Request to send OTP to phone number
    /// </summary>
    public class SendOtpRequest : IFeatureRequest<SendOtpResponse>
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
