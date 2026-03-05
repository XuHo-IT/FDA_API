using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IUserPreferenceRepository
    {
        /// <summary>
        /// Get user preference by userId and key
        /// </summary>
        Task<UserPreference?> GetByUserAndKeyAsync(
            Guid userId,
            string preferenceKey,
            CancellationToken ct = default);

        /// <summary>
        /// Create new preference record
        /// </summary>
        Task<Guid> CreateAsync(
            UserPreference preference,
            CancellationToken ct = default);

        /// <summary>
        /// Update existing preference record
        /// </summary>
        Task<bool> UpdateAsync(
            UserPreference preference,
            CancellationToken ct = default);

        /// <summary>
        /// Delete preference record (optional - for future use)
        /// </summary>
        Task<bool> DeleteAsync(
            Guid id,
            CancellationToken ct = default);
    }
}
