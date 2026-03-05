namespace FDAAPI.App.Common.Models.Auth
{
    /// <summary>
    /// Response status codes for Send OTP feature
    /// </summary>
    public static class SendOtpResponseStatusCode
    {
        public const string Success = "FEAT6_SUCCESS";
        public const string InvalidPhoneNumber = "FEAT6_INVALID_PHONE";
        public const string RateLimitExceeded = "FEAT6_RATE_LIMIT";
        public const string SystemError = "FEAT6_ERROR";
    }
}
