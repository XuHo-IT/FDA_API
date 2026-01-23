using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IUserSubscriptionRepository
    {
        Task<SubscriptionTier> GetUserTierAsync(Guid userId, CancellationToken ct = default);
        Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken ct = default);
        Task<Guid> CreateAsync(UserSubscription subscription, CancellationToken ct = default);
        Task<bool> UpdateAsync(UserSubscription subscription, CancellationToken ct = default);
    }
}