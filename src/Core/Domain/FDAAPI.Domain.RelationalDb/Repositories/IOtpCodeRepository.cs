using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    /// <summary>
    /// Repository interface for OtpCode entity
    /// Manages OTP lifecycle for phone verification
    /// </summary>
    public interface IOtpCodeRepository
    {
        /// <summary>
        /// Create new OTP code (returns generated ID)
        /// Called by SendOtpHandler after generating random code
        /// </summary>
        Task<Guid> CreateAsync(OtpCode otpCode, CancellationToken ct = default);

        /// <summary>
        /// Get latest valid OTP for phone number
        /// Conditions: not used, not expired, ordered by created_at DESC
        /// Used in LoginHandler to verify user's OTP input
        /// </summary>
        Task<OtpCode?> GetLatestValidOtpAsync(string phoneNumber, CancellationToken ct = default);

        /// <summary>
        /// Mark OTP as used after successful login
        /// Set is_used = true, used_at = NOW()
        /// Prevents OTP reuse
        /// </summary>
        Task<bool> MarkAsUsedAsync(Guid otpId, CancellationToken ct = default);

        /// <summary>
        /// Increment attempt count when user enters wrong OTP
        /// Used for rate limiting (e.g., max 3 attempts)
        /// Future: Block OTP after 5 failed attempts
        /// </summary>
        Task<int> IncrementAttemptCountAsync(Guid otpId, CancellationToken ct = default);
    }
}
