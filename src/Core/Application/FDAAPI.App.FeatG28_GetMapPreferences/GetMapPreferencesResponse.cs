using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Map;

namespace FDAAPI.App.FeatG28_GetMapPreferences
{
    public class GetMapPreferencesResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GetMapPreferencesResponseStatusCode StatusCode { get; set; }
        public MapLayerSettings? Settings { get; set; }
    }
}
