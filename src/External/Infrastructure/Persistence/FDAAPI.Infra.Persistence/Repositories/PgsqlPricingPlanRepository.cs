using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlPricingPlanRepository : IPricingPlanRepository
    {
        private readonly AppDbContext _context;

        public PgsqlPricingPlanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PricingPlan?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.PricingPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<PricingPlan?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return await _context.PricingPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code && p.IsActive, ct);
        }

        public async Task<List<PricingPlan>> GetAllActiveAsync(CancellationToken ct = default)
        {
            return await _context.PricingPlans
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToListAsync(ct);
        }
    }
}