using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IUserOAuthProviderRepository
    {
        Task<UserOAuthProvider?> GetByProviderUserIdAsync(
            string provider,
            string providerUserId,
            CancellationToken ct = default);

        Task<UserOAuthProvider?> GetByUserIdAndProviderAsync(
            Guid userId,
            string provider,
            CancellationToken ct = default);

        Task<Guid> CreateAsync(
            UserOAuthProvider oauthProvider,
            CancellationToken ct = default);

        Task<bool> UpdateAsync(
            UserOAuthProvider oauthProvider,
            CancellationToken ct = default);
    }
}






