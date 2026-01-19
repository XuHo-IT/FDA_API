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
    public class PgsqlFloodEventRepository : IFloodEventRepository
    {
        private readonly AppDbContext _context;

        public PgsqlFloodEventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            return await _context.FloodEvents
                .AsNoTracking()
                .Where(e => e.AdministrativeAreaId == administrativeAreaId
                    && e.StartTime >= startDate
                    && e.StartTime < endDate)
                .CountAsync(ct);
        }

        public async Task<List<FloodEvent>> GetByAdministrativeAreaAndPeriodAsync(
            Guid administrativeAreaId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            return await _context.FloodEvents
                .AsNoTracking()
                .Where(e => e.AdministrativeAreaId == administrativeAreaId
                    && e.StartTime >= startDate
                    && e.StartTime < endDate)
                .OrderBy(e => e.StartTime)
                .ToListAsync(ct);
        }

        public async Task<FloodEvent?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.FloodEvents
                .Include(e => e.AdministrativeArea)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<Guid> CreateAsync(FloodEvent floodEvent, CancellationToken ct = default)
        {
            _context.FloodEvents.Add(floodEvent);
            await _context.SaveChangesAsync(ct);
            return floodEvent.Id;
        }

        public async Task<bool> UpdateAsync(FloodEvent floodEvent, CancellationToken ct = default)
        {
            _context.FloodEvents.Update(floodEvent);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var floodEvent = await _context.FloodEvents.FindAsync(new object[] { id }, ct);
            if (floodEvent == null)
            {
                return false;
            }
            _context.FloodEvents.Remove(floodEvent);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<(IEnumerable<FloodEvent> Events, int TotalCount)> GetFloodEventsAsync(
            string? searchTerm,
            Guid? administrativeAreaId,
            DateTime? startDate,
            DateTime? endDate,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _context.FloodEvents
                .Include(e => e.AdministrativeArea)
                .AsNoTracking()
                .AsQueryable();

            if (administrativeAreaId.HasValue)
            {
                query = query.Where(e => e.AdministrativeAreaId == administrativeAreaId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.StartTime <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(e => 
                    (e.AdministrativeArea != null && e.AdministrativeArea.Name.ToLower().Contains(searchTerm)) ||
                    (e.PeakLevel.HasValue && e.PeakLevel.ToString()!.ToLower().Contains(searchTerm)));
            }

            int totalCount = await query.CountAsync(ct);

            var events = await query
                .OrderByDescending(e => e.StartTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (events, totalCount);
        }
    }
}

