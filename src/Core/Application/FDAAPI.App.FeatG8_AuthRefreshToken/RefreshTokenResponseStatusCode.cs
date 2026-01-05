using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG8_AuthRefreshToken
{
    /// <summary>
    /// Response status codes for Refresh Token feature
    /// </summary>
    public static class RefreshTokenResponseStatusCode
    {
        public const string Success = "FEAT8_SUCCESS";
        public const string InvalidToken = "FEAT8_INVALID_TOKEN";
        public const string ExpiredToken = "FEAT8_EXPIRED_TOKEN";
        public const string RevokedToken = "FEAT8_REVOKED_TOKEN";
        public const string UserNotFound = "FEAT8_USER_NOT_FOUND";
        public const string UserBanned = "FEAT8_USER_BANNED";
        public const string SystemError = "FEAT8_ERROR";
    }
}






