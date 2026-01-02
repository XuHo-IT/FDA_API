namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat6.DTOs
{
    /// <summary>
    /// Data Transfer Object for Send OTP request
    /// </summary>
    public class SendOtpRequestDto
    {
        /// <summary>
        /// Phone number or email address
        /// </summary>
        public string Identifier { get; set; } = string.Empty;
    }
}
