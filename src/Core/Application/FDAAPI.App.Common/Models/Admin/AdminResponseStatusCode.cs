namespace FDAAPI.App.Common.Models.Admin
{
    public enum AdminResponseStatusCode
    {
        Success = 0,
        UserNotFound = 1,
        RoleNotFound = 2,
        EmailAlreadyExists = 3,
        PhoneNumberAlreadyExists = 4,
        InvalidInput = 5,
        UnknownError = 99
    }
}

