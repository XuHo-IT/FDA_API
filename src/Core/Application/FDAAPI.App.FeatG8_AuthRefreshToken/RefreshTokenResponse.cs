using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG8_AuthRefreshToken
{
    /// <summary>
    /// Response from refresh token operation
    /// Returns new access token and new refresh token (token rotation)
    /// </summary>
    public class RefreshTokenResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// New JWT access token (60 minutes expiry)
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// New refresh token (7 days expiry)
        /// Old refresh token is automatically revoked (rotation)
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Access token expiration timestamp
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}






