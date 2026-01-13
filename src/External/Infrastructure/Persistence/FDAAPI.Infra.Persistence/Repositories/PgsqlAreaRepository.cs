using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlAreaRepository : IAreaRepository
    {
        private readonly AppDbContext _context;

        public PgsqlAreaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Area?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.Areas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<(List<Area> Areas, int TotalCount)> GetByUserIdAsync(Guid userId, string? searchTerm, int pageNumber, int pageSize, CancellationToken ct)
        {
            var query = _context.Areas
                .AsNoTracking()
                .Where(a => a.UserId == userId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a => a.Name.ToLower().Contains(searchTerm) || 
                                         a.AddressText.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(ct);
            var areas = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (areas, totalCount);
        }

        public async Task<Guid> CreateAsync(Area area, CancellationToken ct)
        {
            _context.Areas.Add(area);
            await _context.SaveChangesAsync(ct);
            return area.Id;
        }

        public async Task<bool> UpdateAsync(Area area, CancellationToken ct)
        {
            _context.Areas.Update(area);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            var area = await _context.Areas.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (area == null) return false;

            _context.Areas.Remove(area);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }
    }
}

