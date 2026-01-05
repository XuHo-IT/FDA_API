using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG6_AuthSendOtp
{
    /// <summary>
    /// Response status codes for Send OTP feature
    /// </summary>
    public static class Feat6ResponseStatusCode
    {
        public const string Success = "FEAT6_SUCCESS";
        public const string InvalidPhoneNumber = "FEAT6_INVALID_PHONE";
        public const string RateLimitExceeded = "FEAT6_RATE_LIMIT";
        public const string SystemError = "FEAT6_ERROR";
    }
}






