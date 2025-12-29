using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    /// <summary>
    /// Repository interface for Role entity operations
    /// Roles are mostly read-only (seeded in migration)
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Get role by code (ADMIN, GOV, USER)
        /// Used when assigning default role during auto-registration
        /// </summary>
        Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default);

        /// <summary>
        /// Get all available roles (for admin UI)
        /// </summary>
        Task<IEnumerable<Role>> GetAllAsync(CancellationToken ct = default);
    }
}
