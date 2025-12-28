using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    /// <summary>
    /// Repository interface for UserRole junction table
    /// Manages many-to-many relationship between Users and Roles
    /// </summary>
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Assign role to user (create user_roles record)
        /// Used during auto-registration to assign USER role
        /// </summary>
        /// <returns>True if assigned, false if already exists</returns>
        Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default);

        /// <summary>
        /// Get all roles for a specific user
        /// Used in LoginHandler to generate JWT role claims
        /// </summary>
        Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);

        /// <summary>
        /// Remove role from user (delete user_roles record)
        /// Future: Admin role management
        /// </summary>
        Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    }
}
