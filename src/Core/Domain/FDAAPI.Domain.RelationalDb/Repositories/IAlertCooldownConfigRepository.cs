using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IAlertCooldownConfigRepository
    {
        Task<AlertCooldownConfig?> GetBySeverityAsync(string severity, CancellationToken ct = default);
        Task<List<AlertCooldownConfig>> GetAllActiveAsync(CancellationToken ct = default);
        Task<int> GetCooldownMinutesAsync(string severity, CancellationToken ct = default);
    }
}