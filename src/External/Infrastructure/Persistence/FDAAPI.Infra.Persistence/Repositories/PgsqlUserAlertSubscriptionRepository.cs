using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlUserAlertSubscriptionRepository : IUserAlertSubscriptionRepository
    {
        private readonly AppDbContext _context;

        public PgsqlUserAlertSubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(UserAlertSubscription entity, CancellationToken ct = default)
        {
            _context.UserAlertSubscriptions.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(UserAlertSubscription entity, CancellationToken ct = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var subscription = await _context.UserAlertSubscriptions.FindAsync(new object[] { id }, ct);
            if (subscription == null)
                return false;

            _context.UserAlertSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<UserAlertSubscription?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.UserAlertSubscriptions
                .Include(s => s.User)
                .Include(s => s.Station)
                .Include(s => s.Area)
                .FirstOrDefaultAsync(s => s.Id == id, ct);
        }

        public async Task<IEnumerable<UserAlertSubscription>> GetByUserIdAsync(
            Guid userId,
            CancellationToken ct = default)
        {
            return await _context.UserAlertSubscriptions
                .AsNoTracking()
                .Include(s => s.Station)
                .Include(s => s.Area)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<UserAlertSubscription>> GetByStationIdAsync(
            Guid stationId,
            CancellationToken ct = default)
        {
            return await _context.UserAlertSubscriptions
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.StationId == stationId)
                .ToListAsync(ct);
        }

        public async Task<bool> IsUserSubscribedAsync(
            Guid userId,
            Guid stationId,
            CancellationToken ct = default)
        {
            return await _context.UserAlertSubscriptions
                .AsNoTracking()
                .AnyAsync(s => s.UserId == userId && s.StationId == stationId, ct);
        }

        public async Task<(List<UserAlertSubscription> Items, int TotalCount)> GetAllWithPaginationAsync(
            int page,
            int pageSize,
            Guid? userId = null,
            Guid? stationId = null,
            CancellationToken ct = default)
        {
            var query = _context.UserAlertSubscriptions
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Station)
                .Include(s => s.Area)
                .AsQueryable();

            // Apply filters
            if (userId.HasValue)
            {
                query = query.Where(s => s.UserId == userId.Value);
            }

            if (stationId.HasValue)
            {
                query = query.Where(s => s.StationId == stationId.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination
            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
        public async Task<int> CountActiveSubscribersAsync(CancellationToken ct = default)
        {
            return await _context.UserAlertSubscriptions
                .AsNoTracking()
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync(ct);
        }

        public async Task<int> CountNewSubscribersAsync(DateTime fromDate, CancellationToken ct = default)
        {
            return await _context.UserAlertSubscriptions
                .AsNoTracking()
                .Where(s => s.CreatedAt >= fromDate)
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync(ct);
        }

        public async Task<IEnumerable<UserAlertSubscription>> GetByAreaIdAsync(
            Guid areaId,
            CancellationToken ct = default)
                {
                    return await _context.UserAlertSubscriptions
                        .Include(s => s.User)
                        .Include(s => s.Area)
                        .Where(s => s.AreaId == areaId)
                        .AsNoTracking()
                        .ToListAsync(ct);
                }

        public async Task<IEnumerable<UserAlertSubscription>> GetByAreaIdsAsync(
            List<Guid> areaIds,
            CancellationToken ct = default)
        {
            return await _context.UserAlertSubscriptions
                .Include(s => s.User)
                .Include(s => s.Area)
                .Where(s => s.AreaId.HasValue && areaIds.Contains(s.AreaId.Value))
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}