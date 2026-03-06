using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlSensorIncidentRepository : ISensorIncidentRepository
    {
        private readonly AppDbContext _context;

        public PgsqlSensorIncidentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SensorIncident?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.SensorIncidents
                .Include(i => i.Station)
                .Include(i => i.AssignedUser)
                .FirstOrDefaultAsync(i => i.Id == id, ct);
        }

        public async Task<List<SensorIncident>> GetAllAsync(
            string? status,
            string? incidentType,
            Guid? assignedTo,
            Guid? stationId,
            CancellationToken ct = default)
        {
            var query = _context.SensorIncidents
                .Include(i => i.Station)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(i => i.Status == status);

            if (!string.IsNullOrWhiteSpace(incidentType))
                query = query.Where(i => i.IncidentType == incidentType);

            if (assignedTo.HasValue)
                query = query.Where(i => i.AssignedTo == assignedTo);

            if (stationId.HasValue)
                query = query.Where(i => i.StationId == stationId);

            return await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Guid> CreateAsync(SensorIncident incident, CancellationToken ct = default)
        {
            _context.SensorIncidents.Add(incident);
            await _context.SaveChangesAsync(ct);
            return incident.Id;
        }

        public async Task<bool> UpdateAsync(SensorIncident incident, CancellationToken ct = default)
        {
            _context.SensorIncidents.Update(incident);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<SensorIncident>> GetActiveByStationAsync(Guid stationId, CancellationToken ct = default)
        {
            return await _context.SensorIncidents
                .Where(i => i.StationId == stationId && (i.Status == "open" || i.Status == "in_progress"))
                .ToListAsync(ct);
        }
    }
}
