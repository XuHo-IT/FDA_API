using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Services
{
    /// <summary>
    /// Abstraction for sending OTP codes via SMS or Email.
    /// Implementation resides in Infrastructure layer.
    /// </summary>
    public interface IOtpSender
    {
        /// <summary>
        /// Send OTP code to the specified identifier (phone or email)
        /// </summary>
        /// <param name="identifier">Phone number or email address</param>
        /// <param name="identifierType">"phone" or "email"</param>
        /// <param name="otpCode">The generated OTP code</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if sent successfully</returns>
        Task<bool> SendOtpAsync(
            string identifier,
            string identifierType,
            string otpCode,
            CancellationToken ct = default);
    }
}
