using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlFloodReportMediaRepository : IFloodReportMediaRepository
    {
        private readonly AppDbContext _context;

        public PgsqlFloodReportMediaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(FloodReportMedia entity, CancellationToken ct = default)
        {
            _context.FloodReportMedia.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var media = await _context.FloodReportMedia.FindAsync(new object[] { id }, ct);
            if (media == null)
            {
                return false;
            }
            _context.FloodReportMedia.Remove(media);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<FloodReportMedia?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.FloodReportMedia
                .FirstOrDefaultAsync(m => m.Id == id, ct);
        }

        public async Task<bool> UpdateAsync(FloodReportMedia entity, CancellationToken ct = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<FloodReportMedia>> GetByReportIdAsync(Guid reportId, CancellationToken ct = default)
        {
            return await _context.FloodReportMedia
                .AsNoTracking()
                .Where(m => m.FloodReportId == reportId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(ct);
        }
    }
}

