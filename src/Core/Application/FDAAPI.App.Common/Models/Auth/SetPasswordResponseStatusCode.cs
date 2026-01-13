namespace FDAAPI.App.Common.Models.Auth
{
    public enum SetPasswordResponseStatusCode
    {
        Success = 0,
        UserNotFound = 1,
        PasswordAlreadyExists = 2,
        NewPasswordInvalid = 3,
        PasswordMismatch = 4,
        EmailInvalid = 5,
        EmailAlreadyExists = 6,
        UnknownError = 99
    }
}
