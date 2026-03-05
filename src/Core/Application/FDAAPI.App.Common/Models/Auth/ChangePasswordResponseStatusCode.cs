namespace FDAAPI.App.Common.Models.Auth
{
    public enum ChangePasswordResponseStatusCode
    {
        Success = 0,
        UserNotFound = 1,
        CurrentPasswordIncorrect = 2,
        NewPasswordInvalid = 3,
        PasswordMismatch = 4,
        SameAsCurrentPassword = 5,
        UnknownError = 99
    }
}
