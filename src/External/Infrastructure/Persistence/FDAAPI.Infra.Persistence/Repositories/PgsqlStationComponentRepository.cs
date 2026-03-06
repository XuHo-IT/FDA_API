using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlStationComponentRepository : IStationComponentRepository
    {
        private readonly AppDbContext _context;

        public PgsqlStationComponentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StationComponent>> GetByStationIdAsync(Guid stationId, CancellationToken ct = default)
        {
            return await _context.StationComponents
                .AsNoTracking()
                .Where(c => c.StationId == stationId)
                .OrderBy(c => c.ComponentType)
                .ToListAsync(ct);
        }

        public async Task<StationComponent?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.StationComponents
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<bool> ExistsByTypeAsync(Guid stationId, string componentType, CancellationToken ct = default)
        {
            return await _context.StationComponents
                .AnyAsync(c => c.StationId == stationId && c.ComponentType == componentType, ct);
        }

        public async Task<StationComponent> CreateAsync(StationComponent component, CancellationToken ct = default)
        {
            _context.StationComponents.Add(component);
            await _context.SaveChangesAsync(ct);
            return component;
        }

        public async Task<StationComponent> UpdateAsync(StationComponent component, CancellationToken ct = default)
        {
            _context.Entry(component).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return component;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var component = await _context.StationComponents.FindAsync(new object[] { id }, ct);
            if (component == null)
                return false;

            _context.StationComponents.Remove(component);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
