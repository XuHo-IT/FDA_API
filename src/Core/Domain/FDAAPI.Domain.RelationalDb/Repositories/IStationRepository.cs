using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IStationRepository
    {
        Task<Station?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(Station entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(Station entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Station>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default);
        Task<(IEnumerable<Station> Stations, int TotalCount)> GetStationsAsync(
            string? searchTerm,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
    }
}
