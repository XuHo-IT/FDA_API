using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlAlertTemplateRepository : IAlertTemplateRepository
    {
        private readonly AppDbContext _context;

        public PgsqlAlertTemplateRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AlertTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.AlertTemplates
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<Guid> CreateAsync(AlertTemplate entity, CancellationToken ct = default)
        {
            _context.AlertTemplates.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(AlertTemplate entity, CancellationToken ct = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var template = await _context.AlertTemplates.FindAsync(new object[] { id }, ct);
            if (template == null)
                return false;

            _context.AlertTemplates.Remove(template);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<AlertTemplate>> GetAllAsync(
            bool? isActive = null,
            string? channel = null,
            string? severity = null,
            CancellationToken ct = default)
        {
            var query = _context.AlertTemplates.AsNoTracking();

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            if (!string.IsNullOrEmpty(channel))
                query = query.Where(t => t.Channel == channel);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(t => t.Severity == severity);

            return await query
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Name)
                .ToListAsync(ct);
        }

        public async Task<AlertTemplate?> GetByChannelAndSeverityAsync(
            string channel,
            string? severity,
            CancellationToken ct = default)
        {
            // First try to find exact match (channel + severity)
            var template = await _context.AlertTemplates
                .AsNoTracking()
                .Where(t => t.Channel == channel && t.Severity == severity && t.IsActive)
                .OrderBy(t => t.SortOrder)
                .FirstOrDefaultAsync(ct);

            if (template != null)
                return template;

            // Fallback: find template with null severity (applies to all)
            return await _context.AlertTemplates
                .AsNoTracking()
                .Where(t => t.Channel == channel && t.Severity == null && t.IsActive)
                .OrderBy(t => t.SortOrder)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<AlertTemplate?> GetByChannelAsync(
            string channel,
            CancellationToken ct = default)
        {
            return await _context.AlertTemplates
                .AsNoTracking()
                .Where(t => t.Channel == channel && t.IsActive)
                .OrderBy(t => t.SortOrder)
                .FirstOrDefaultAsync(ct);
        }
    }
}
