using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG6
{
    /// <summary>
    /// Response from sending OTP
    /// </summary>
    public class SendOtpResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        // For development/testing only - remove in production
        public string? OtpCode { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
