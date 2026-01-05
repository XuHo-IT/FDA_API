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
    public class PgsqlUserOAuthProviderRepository : IUserOAuthProviderRepository
    {
        private readonly AppDbContext _context;

        public PgsqlUserOAuthProviderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserOAuthProvider?> GetByProviderUserIdAsync(string provider, string providerUserId, CancellationToken ct = default)
        {
            return await _context.UserOAuthProviders
                .Include(o => o.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(o => o.Provider == provider && o.ProviderUserId == providerUserId, ct);
        }

        public async Task<UserOAuthProvider?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken ct = default)
        {
            return await _context.UserOAuthProviders
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Provider == provider, ct);
        }

        public async Task<Guid> CreateAsync(UserOAuthProvider oauthProvider, CancellationToken ct = default)
        {
            oauthProvider.Id = Guid.NewGuid();
            oauthProvider.CreatedAt = DateTime.UtcNow;
            oauthProvider.UpdatedAt = DateTime.UtcNow;

            await _context.UserOAuthProviders.AddAsync(oauthProvider, ct);
            await _context.SaveChangesAsync(ct);

            return oauthProvider.Id;
        }

        public async Task<bool> UpdateAsync(UserOAuthProvider oauthProvider, CancellationToken ct = default)
        {
            oauthProvider.UpdatedAt = DateTime.UtcNow;

            _context.UserOAuthProviders.Update(oauthProvider);
            var rowsAffected = await _context.SaveChangesAsync(ct);

            return rowsAffected > 0;
        }
    }
}






