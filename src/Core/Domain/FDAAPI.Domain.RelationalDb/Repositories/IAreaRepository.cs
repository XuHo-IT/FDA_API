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
        Task<(List<Area> Areas, int TotalCount)> GetAdminAreasAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken ct);
        Task<Guid> CreateAsync(Area area, CancellationToken ct);
        Task<bool> UpdateAsync(Area area, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);

        // NEW: For area limit check
        Task<int> CountByUserIdAsync(Guid userId, CancellationToken ct = default);

        // NEW: For name uniqueness check (case-insensitive)
        Task<Area?> GetByUserIdAndNameAsync(Guid userId, string name, CancellationToken ct = default);

        // NEW: For duplicate location prevention (Haversine)
        Task<List<Area>> GetUserAreasWithinRadiusAsync(Guid userId, decimal latitude, decimal longitude, int radiusMeters, CancellationToken ct = default);
        Task<List<Area>> GetAreasContainingStationAsync(
            Guid stationId,
            decimal stationLat,
            decimal stationLng,
            CancellationToken ct = default);
    }
}

