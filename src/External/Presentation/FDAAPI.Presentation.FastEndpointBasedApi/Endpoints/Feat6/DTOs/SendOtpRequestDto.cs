namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat6.DTOs
{
    /// <summary>
    /// Data Transfer Object for Send OTP request
    /// </summary>
    public class SendOtpRequestDto
    {
        /// <summary>
        /// Phone number to send OTP to (E.164 format recommended)
        /// Example: +84901234567
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
