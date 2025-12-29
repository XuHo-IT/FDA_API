using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    /// <summary>
    /// Repository interface for RefreshToken entity
    /// Manages refresh token lifecycle (create, validate, revoke)
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Create new refresh token (returns generated ID)
        /// Called after successful login
        /// </summary>
        Task<Guid> CreateAsync(RefreshToken token, CancellationToken ct = default);

        /// <summary>
        /// Get refresh token by token string (for validation)
        /// Used in RefreshTokenHandler to verify token validity
        /// </summary>
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

        /// <summary>
        /// Revoke single refresh token (set is_revoked = true)
        /// Used during logout or token refresh (rotation)
        /// </summary>
        Task<bool> RevokeTokenAsync(string token, CancellationToken ct = default);

        /// <summary>
        /// Revoke all refresh tokens for a user (logout from all devices)
        /// Used when user clicks "Logout from all devices"
        /// </summary>
        Task<bool> RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default);

        /// <summary>
        /// Cleanup expired refresh tokens (background job)
        /// Delete tokens that expired more than 30 days ago
        /// Future: Call from Quartz scheduled job
        /// </summary>
        Task<bool> CleanupExpiredTokensAsync(CancellationToken ct = default);
    }
}
