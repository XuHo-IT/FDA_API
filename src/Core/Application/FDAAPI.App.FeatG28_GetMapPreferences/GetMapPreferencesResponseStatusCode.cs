using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG28_GetMapPreferences
{
    public enum GetMapPreferencesResponseStatusCode
    {
        Success = 0,
        UserNotFound = 1,
        InvalidJsonFormat = 2,
        UnknownError = 99
    }
}
