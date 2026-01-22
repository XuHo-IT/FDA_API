using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IUserAlertSubscriptionRepository
    {
        Task<Guid> CreateAsync(UserAlertSubscription entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(UserAlertSubscription entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<UserAlertSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<UserAlertSubscription>> GetByUserIdAsync(
            Guid userId,
            CancellationToken ct = default);
        Task<IEnumerable<UserAlertSubscription>> GetByStationIdAsync(
            Guid stationId,
            CancellationToken ct = default);
        Task<bool> IsUserSubscribedAsync(
            Guid userId,
            Guid stationId,
            CancellationToken ct = default);
        Task<(List<UserAlertSubscription> Items, int TotalCount)> GetAllWithPaginationAsync(
            int page,
            int pageSize,
            Guid? userId = null,
            Guid? stationId = null,
            CancellationToken ct = default);
        Task<int> CountActiveSubscribersAsync(CancellationToken ct = default);
        Task<int> CountNewSubscribersAsync(DateTime fromDate, CancellationToken ct = default);
        Task<IEnumerable<UserAlertSubscription>> GetByAreaIdAsync(
            Guid areaId,
            CancellationToken ct = default);
        Task<IEnumerable<UserAlertSubscription>> GetByAreaIdsAsync(
            List<Guid> areaIds,
            CancellationToken ct = default);
    }
}