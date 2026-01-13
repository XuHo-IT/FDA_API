namespace FDAAPI.App.Common.Models.Auth
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
