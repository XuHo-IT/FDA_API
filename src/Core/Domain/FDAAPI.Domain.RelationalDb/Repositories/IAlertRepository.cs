using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IAlertRepository
    {
        Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(Alert entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(Alert entity, CancellationToken ct = default);

        // Get active alerts for a station
        Task<IEnumerable<Alert>> GetActiveAlertsByStationAsync(
            Guid stationId,
            CancellationToken ct = default);

        // Get alerts by severity and status
        Task<IEnumerable<Alert>> GetAlertsBySeverityAsync(
            string severity,
            string status = "open",
            CancellationToken ct = default);

        // Get unnotified alerts (for batch processing)
        Task<IEnumerable<Alert>> GetUnnotifiedAlertsAsync(
            int take = 100,
            CancellationToken ct = default);
    }
}