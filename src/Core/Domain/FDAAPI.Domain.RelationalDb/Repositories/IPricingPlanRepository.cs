using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IPricingPlanRepository
    {
        Task<PricingPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PricingPlan?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<List<PricingPlan>> GetAllActiveAsync(CancellationToken ct = default);
    }
}