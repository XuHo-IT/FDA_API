using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlUserSubscriptionRepository : IUserSubscriptionRepository
    {
        private readonly AppDbContext _context;

        public PgsqlUserSubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionTier> GetUserTierAsync(Guid userId, CancellationToken ct = default)
        {
            var activeSubscription = await _context.UserSubscriptions
                .Where(s => s.UserId == userId &&
                            s.Status == "active" &&
                            s.EndDate > DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(ct);

            return activeSubscription?.Plan?.Tier ?? SubscriptionTier.Free;
        }

        public async Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId, CancellationToken ct)
        {
            return await _context.UserSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId &&
                            s.Status == "active" &&
                            s.EndDate > DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Guid> CreateAsync(UserSubscription subscription, CancellationToken ct)
        {
            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync(ct);
            return subscription.Id;
        }

        public async Task<bool> UpdateAsync(UserSubscription subscription, CancellationToken ct)
        {
            _context.Entry(subscription).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}