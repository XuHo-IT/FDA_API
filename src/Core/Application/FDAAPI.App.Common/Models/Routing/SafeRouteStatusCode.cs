using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Models.Routing
{
    public enum SafeRouteStatusCode
    {
        Success = 200,
        BadRequest = 400,
        Unauthorized = 401,
        NotFound = 404,
        RouteBlocked = 422,
        ServiceUnavailable = 503,
        UnknownError = 500
    }
}
