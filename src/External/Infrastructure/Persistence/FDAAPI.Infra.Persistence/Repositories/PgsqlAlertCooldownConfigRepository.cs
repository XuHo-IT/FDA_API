using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlAlertCooldownConfigRepository : IAlertCooldownConfigRepository
    {
        private readonly AppDbContext _context;

        public PgsqlAlertCooldownConfigRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AlertCooldownConfig?> GetBySeverityAsync(
            string severity,
            CancellationToken ct = default)
        {
            return await _context.AlertCooldownConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Severity == severity && c.IsActive, ct);
        }

        public async Task<List<AlertCooldownConfig>> GetAllActiveAsync(
            CancellationToken ct = default)
        {
            return await _context.AlertCooldownConfigs
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Severity)
                .ToListAsync(ct);
        }

        public async Task<int> GetCooldownMinutesAsync(
            string severity,
            CancellationToken ct = default)
        {
            var config = await GetBySeverityAsync(severity, ct);

            // Return configured value or default based on severity
            if (config != null)
            {
                return config.CooldownMinutes;
            }

            // Fallback defaults if no config found
            return severity.ToLower() switch
            {
                "critical" => 5,
                "warning" => 10,
                "caution" => 20,
                "info" => 30,
                _ => 10
            };
        }
    }
}