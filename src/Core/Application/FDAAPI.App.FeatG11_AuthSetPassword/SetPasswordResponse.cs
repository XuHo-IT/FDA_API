using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG11_AuthSetPassword
{
    public class SetPasswordResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SetPasswordResponseStatusCode StatusCode { get; set; }
    }
}






