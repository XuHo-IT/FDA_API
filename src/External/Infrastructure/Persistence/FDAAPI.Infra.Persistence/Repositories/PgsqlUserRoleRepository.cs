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
    /// PostgreSQL implementation of IUserRoleRepository
    /// Manages many-to-many relationship between Users and Roles
    /// </summary>
    public class PgsqlUserRoleRepository : IUserRoleRepository
    {
        private readonly AppDbContext _context;

        public PgsqlUserRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        {
            // Check if relationship already exists
            var existingUserRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

            if (existingUserRole != null)
            {
                return false; // Already assigned
            }

            // Create new user-role relationship
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

            if (userRole == null)
            {
                return false;
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}






