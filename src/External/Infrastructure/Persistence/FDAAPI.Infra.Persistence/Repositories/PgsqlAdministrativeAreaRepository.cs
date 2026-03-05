using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlAdministrativeAreaRepository : IAdministrativeAreaRepository
    {
        private readonly AppDbContext _context;

        public PgsqlAdministrativeAreaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdministrativeArea?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.AdministrativeAreas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<List<AdministrativeArea>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
        {
            return await _context.AdministrativeAreas
                .AsNoTracking()
                .Where(a => ids.Contains(a.Id))
                .ToListAsync(ct);
        }

        public async Task<List<AdministrativeArea>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.AdministrativeAreas
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<List<AdministrativeArea>> GetByLevelAsync(string level, CancellationToken ct = default)
        {
            return await _context.AdministrativeAreas
                .AsNoTracking()
                .Where(a => a.Level == level)
                .ToListAsync(ct);
        }

        public async Task<Guid> CreateAsync(AdministrativeArea area, CancellationToken ct = default)
        {
            _context.AdministrativeAreas.Add(area);
            await _context.SaveChangesAsync(ct);
            return area.Id;
        }

        public async Task<bool> UpdateAsync(AdministrativeArea area, CancellationToken ct = default)
        {
            _context.AdministrativeAreas.Update(area);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var area = await _context.AdministrativeAreas.FindAsync(new object[] { id }, ct);
            if (area == null)
            {
                return false;
            }
            _context.AdministrativeAreas.Remove(area);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<(IEnumerable<AdministrativeArea> Areas, int TotalCount)> GetAdministrativeAreasAsync(
            string? searchTerm,
            string? level,
            Guid? parentId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _context.AdministrativeAreas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(level))
            {
                query = query.Where(a => a.Level == level);
            }

            if (parentId.HasValue)
            {
                query = query.Where(a => a.ParentId == parentId.Value);
            }
            else if (parentId == null && level != "city")
            {
                // If parentId is explicitly null (not just not provided), filter for top-level items
                // This is useful for getting cities only
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(a => a.Name.ToLower().Contains(searchTerm)
                                      || (a.Code != null && a.Code.ToLower().Contains(searchTerm)));
            }

            int totalCount = await query.CountAsync(ct);

            var areas = await query
                .OrderBy(a => a.Level)
                .ThenBy(a => a.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (areas, totalCount);
        }
    }
}

