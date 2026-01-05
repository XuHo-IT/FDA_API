using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG17_AuthCheckIdentifier
{
    /// <summary>
    /// Response indicating authentication method required for identifier
    /// </summary>
    public class CheckIdentifierResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Type of identifier: "phone" or "email"
        /// </summary>
        public string? IdentifierType { get; set; }

        /// <summary>
        /// Whether account exists for this identifier
        /// </summary>
        public bool AccountExists { get; set; }

        /// <summary>
        /// Whether account has password set
        /// </summary>
        public bool HasPassword { get; set; }

        /// <summary>
        /// Required authentication method: "password" or "otp"
        /// </summary>
        public string? RequiredMethod { get; set; }
    }
}






