using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    /// <summary>
    /// PostgreSQL implementation of IRefreshTokenRepository
    /// Manages refresh token lifecycle (create, validate, revoke)
    /// </summary>
    public class PgsqlRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public PgsqlRefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(RefreshToken token, CancellationToken ct = default)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync(ct);
            return token.Id;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);
        }

        public async Task<bool> RevokeTokenAsync(string token, CancellationToken ct = default)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);

            if (refreshToken == null)
            {
                return false;
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CleanupExpiredTokensAsync(CancellationToken ct = default)
        {
            // Remove tokens that expired more than 30 days ago
            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < cutoffDate)
                .ToListAsync(ct);

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(ct);

            return true;
        }
    }
}
