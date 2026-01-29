using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Routing;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.App.FeatG74_RequestSafeRoute
{
    public class SafeRouteResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SafeRouteStatusCode StatusCode { get; set; }
        public SafeRouteData? Data { get; set; }
    }

    public class SafeRouteData
    {
        public RouteDto PrimaryRoute { get; set; } = new();
        public List<RouteDto> AlternativeRoutes { get; set; } = new();
        public List<FloodWarningDto> FloodWarnings { get; set; } = new();
        public RouteSafetyStatus SafetyStatus { get; set; }
    }

}
