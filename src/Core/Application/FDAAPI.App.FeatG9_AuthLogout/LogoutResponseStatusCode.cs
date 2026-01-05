using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG9_AuthLogout
{
    /// <summary>
    /// Response status codes for Logout feature
    /// </summary>
    public static class LogoutResponseStatusCode
    {
        public const string Success = "FEAT9_SUCCESS";
        public const string SuccessAllDevices = "FEAT9_SUCCESS_ALL_DEVICES";
        public const string TokenNotFound = "FEAT9_TOKEN_NOT_FOUND";
        public const string AlreadyRevoked = "FEAT9_ALREADY_REVOKED";
        public const string SystemError = "FEAT9_ERROR";
    }
}






