using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG17
{
    /// <summary>
    /// Request to check if an identifier (phone/email) exists and requires password or OTP
    /// </summary>
    public class CheckIdentifierRequest : IFeatureRequest<CheckIdentifierResponse>
    {
        /// <summary>
        /// Phone number or email address
        /// </summary>
        public string Identifier { get; set; } = string.Empty;
    }
}
