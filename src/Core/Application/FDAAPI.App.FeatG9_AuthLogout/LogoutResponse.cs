using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG9_AuthLogout
{
    /// <summary>
    /// Response from logout operation
    /// </summary>
    public class LogoutResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Number of tokens revoked
        /// 1 = single device logout
        /// N = logout from N devices
        /// </summary>
        public int TokensRevoked { get; set; }
    }
}






