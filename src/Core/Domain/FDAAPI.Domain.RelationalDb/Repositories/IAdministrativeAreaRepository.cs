using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IAdministrativeAreaRepository
    {
        Task<AdministrativeArea?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default);

        Task<List<AdministrativeArea>> GetByIdsAsync(
            List<Guid> ids,
            CancellationToken ct = default);

        Task<List<AdministrativeArea>> GetAllAsync(
            CancellationToken ct = default);

        Task<List<AdministrativeArea>> GetByLevelAsync(
            string level, // "ward", "district", "city"
            CancellationToken ct = default);

        Task<Guid> CreateAsync(
            AdministrativeArea area,
            CancellationToken ct = default);

        Task<bool> UpdateAsync(
            AdministrativeArea area,
            CancellationToken ct = default);

        Task<bool> DeleteAsync(
            Guid id,
            CancellationToken ct = default);

        Task<(IEnumerable<AdministrativeArea> Areas, int TotalCount)> GetAdministrativeAreasAsync(
            string? searchTerm,
            string? level,
            Guid? parentId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
    }
}

