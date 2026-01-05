using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG11_AuthSetPassword
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






