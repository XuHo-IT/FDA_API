using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgslStationRepository : IStationRepository
    {
        private readonly AppDbContext _context;
        public PgslStationRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> CreateAsync(Station entity, CancellationToken ct = default)
        {
           _context.Stations.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station == null)
            {
                return false;
            }
            _context.Stations.Remove(station);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Station?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Stations.FindAsync(id);
        }

        public async Task<(IEnumerable<Station> Stations, int TotalCount)> GetStationsAsync(
    string? searchTerm,
    string? status,
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
        {
            var query = _context.Stations.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.Status == status);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(s => s.Code.ToLower().Contains(searchTerm)
                                      || (s.Name != null && s.Name.ToLower().Contains(searchTerm)));
            }

            int totalCount = await query.CountAsync(ct);

            var stations = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (stations, totalCount);
        }

        public async Task<IEnumerable<Station>> ListAsync(int skip = 0, int take = 50, CancellationToken ct = default)
        {
            return await _context.Stations.Skip(skip).Take(take).ToListAsync();
        }

        public async Task<bool> UpdateAsync(Station entity, CancellationToken ct = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Station>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Stations
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}
