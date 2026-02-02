using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Models.Auth
{
    public enum ResetPasswordResponseStatusCode
    {
        Success = 0,
        UserNotFound = 1,
        NewPasswordInvalid = 2,
        PasswordMismatch = 3,
        SameAsCurrentPassword = 4,
        UnknownError = 99
    }
}
