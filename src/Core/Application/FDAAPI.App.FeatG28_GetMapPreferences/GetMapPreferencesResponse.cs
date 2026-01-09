using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;

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
