using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    /// <summary>
    /// PostgreSQL implementation of IUserRepository
    /// Handles CRUD operations for User entity
    /// </summary>
    public class PgsqlUserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public PgsqlUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Users.FindAsync(new object[] { id }, ct);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, ct);
        }

        public async Task<Guid> CreateAsync(User user, CancellationToken ct = default)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);
            return user.Id;
        }

        public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }
        public async Task<IEnumerable<User>> GetUsersWithRolesByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
        {
            if (userIds == null || !userIds.Any())
                return Enumerable.Empty<User>();

            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => userIds.Contains(u.Id))
                .AsNoTracking()
                .ToListAsync(ct);
        }
        public async Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, ct);
        }
        public async Task<string?> GetUserFullNameAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<string?> GetAvatarUrlAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.AvatarUrl)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(
            string? searchTerm,
            string? role,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(searchTerm)) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Code == role));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(u => u.Status == status);
            }

            var totalCount = await query.CountAsync(ct);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (users, totalCount);
        }

    }
}






