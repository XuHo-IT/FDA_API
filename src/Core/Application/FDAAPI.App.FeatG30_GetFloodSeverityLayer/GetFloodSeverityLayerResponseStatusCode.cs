using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG30_GetFloodSeverityLayer
{
    public enum GetFloodSeverityLayerResponseStatusCode
    {
        Success = 0,
        NoStationsFound = 1,
        InvalidBounds = 2,
        UnknownError = 99
    }
}
