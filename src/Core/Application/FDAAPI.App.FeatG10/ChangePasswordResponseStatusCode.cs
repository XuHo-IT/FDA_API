using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG10
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
