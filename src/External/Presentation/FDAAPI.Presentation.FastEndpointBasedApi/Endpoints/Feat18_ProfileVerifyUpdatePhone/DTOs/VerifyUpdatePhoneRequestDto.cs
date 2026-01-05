namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat18_ProfileVerifyUpdatePhone.DTOs{
    public class VerifyUpdatePhoneRequestDto
    {
        /// <summary>
        /// New phone number to be verified and updated
        /// </summary>
        public string NewPhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Otp code sent to the new phone number for verification
        /// </summary>
        public string OtpCode { get; set; } = string.Empty;
    }
}








