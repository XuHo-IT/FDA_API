using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG15
{
    public enum UpdateProfileResponseStatusCode
    {
        Success = 0,
        UserNotFound = 1,
        InvalidInput = 2,
        UnknownError = 99
    }
}
