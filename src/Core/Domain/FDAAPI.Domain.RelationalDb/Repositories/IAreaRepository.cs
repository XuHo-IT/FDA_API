using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IAreaRepository
    {
        Task<Area?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<(List<Area> Areas, int TotalCount)> GetByUserIdAsync(Guid userId, string? searchTerm, int pageNumber, int pageSize, CancellationToken ct);
        Task<Guid> CreateAsync(Area area, CancellationToken ct);
        Task<bool> UpdateAsync(Area area, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }
}

