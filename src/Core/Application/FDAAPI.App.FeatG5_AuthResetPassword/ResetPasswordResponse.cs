using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Auth;

namespace FDAAPI.App.FeatG5_AuthResetPassword
{
    public class ResetPasswordResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ResetPasswordResponseStatusCode StatusCode { get; set; }
    }
}
