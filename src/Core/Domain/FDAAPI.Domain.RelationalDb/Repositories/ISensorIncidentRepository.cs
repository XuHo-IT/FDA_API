using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface ISensorIncidentRepository
    {
        Task<SensorIncident?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<SensorIncident>> GetAllAsync(string? status, string? incidentType, Guid? assignedTo, Guid? stationId, CancellationToken ct = default);
        Task<Guid> CreateAsync(SensorIncident incident, CancellationToken ct = default);
        Task<bool> UpdateAsync(SensorIncident incident, CancellationToken ct = default);
        Task<List<SensorIncident>> GetActiveByStationAsync(Guid stationId, CancellationToken ct = default);
    }
}
