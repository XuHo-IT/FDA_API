using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG29_UpdateMapPreferences
{
    public class UpdateMapPreferencesResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UpdateMapPreferencesResponseStatusCode StatusCode { get; set; }
    }
}
