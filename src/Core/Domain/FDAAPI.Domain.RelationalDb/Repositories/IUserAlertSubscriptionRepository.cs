using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IUserAlertSubscriptionRepository
    {
        Task<Guid> CreateAsync(UserAlertSubscription entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(UserAlertSubscription entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

        Task<UserAlertSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default);

        // Get subscriptions for a user
        Task<IEnumerable<UserAlertSubscription>> GetByUserIdAsync(
            Guid userId,
            CancellationToken ct = default);

        // Get users subscribed to a station
        Task<IEnumerable<UserAlertSubscription>> GetByStationIdAsync(
            Guid stationId,
            CancellationToken ct = default);

        // Check if user is subscribed to station
        Task<bool> IsUserSubscribedAsync(
            Guid userId,
            Guid stationId,
            CancellationToken ct = default);
    }
}