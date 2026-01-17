using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IAlertRuleRepository
    {
        Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(AlertRule entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(AlertRule entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

        // Get all active rules
        Task<IEnumerable<AlertRule>> GetActiveRulesAsync(CancellationToken ct = default);

        // Get rules by station
        Task<IEnumerable<AlertRule>> GetByStationIdAsync(
            Guid stationId,
            CancellationToken ct = default);

        // Get rules by area
        //Task<IEnumerable<AlertRule>> GetByAreaIdAsync(
        //    Guid areaId,
        //    CancellationToken ct = default);
    }
}