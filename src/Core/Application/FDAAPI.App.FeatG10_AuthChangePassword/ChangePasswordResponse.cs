using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Auth;

namespace FDAAPI.App.FeatG10_AuthChangePassword
{
    public class ChangePasswordResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ChangePasswordResponseStatusCode StatusCode { get; set; }
    }
}






