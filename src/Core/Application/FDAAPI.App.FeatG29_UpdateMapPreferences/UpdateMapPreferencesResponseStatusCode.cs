using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG29_UpdateMapPreferences
{
    public enum UpdateMapPreferencesResponseStatusCode
    {
        Success = 0,
        ValidationFailed = 1,
        UserNotFound = 2,
        UnknownError = 99
    }
}
