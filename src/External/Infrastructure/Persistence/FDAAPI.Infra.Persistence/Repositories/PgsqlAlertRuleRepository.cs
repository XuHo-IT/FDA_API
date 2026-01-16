using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Enums;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlAlertRuleRepository : IAlertRuleRepository
    {
        private readonly AppDbContext _context;

        public PgsqlAlertRuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AlertRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.AlertRules
                .Include(r => r.Station)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<Guid> CreateAsync(AlertRule entity, CancellationToken ct = default)
        {
            _context.AlertRules.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(AlertRule entity, CancellationToken ct = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var rule = await _context.AlertRules.FindAsync(new object[] { id }, ct);
            if (rule == null)
                return false;

            _context.AlertRules.Remove(rule);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<AlertRule>> GetActiveRulesAsync(CancellationToken ct = default)
        {
            return await _context.AlertRules
                .AsNoTracking()
                .Include(r => r.Station)
                .Where(r => r.IsActive)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<AlertRule>> GetByStationIdAsync(
            Guid stationId,
            CancellationToken ct = default)
        {
            return await _context.AlertRules
                .AsNoTracking()
                .Include(r => r.Station)
                .Where(r => r.StationId == stationId && r.IsActive)
                .ToListAsync(ct);
        }

        //public async Task<IEnumerable<AlertRule>> GetByAreaIdAsync(
        //    Guid areaId,
        //    CancellationToken ct = default)
        //{
        //    // FIXED: AlertRule doesn't have AreaId field, so we need to query via Station.AreaId
        //    //return await _context.AlertRules
        //    //    .AsNoTracking()
        //    //    .Include(r => r.Station)
        //    //    .Where(r => r.Station!.AreaId == areaId && r.IsActive)
        //    //    .ToListAsync(ct);
        //}
    }
}