using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    /// <summary>
    /// Repository interface for User entity operations
    /// Defines contract for user data access
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Get user by unique identifier
        /// </summary>
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Get user by email address (for email+password login)
        /// </summary>
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

        /// <summary>
        /// Get user by phone number (for phone+OTP login)
        /// </summary>
        Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);

        /// <summary>
        /// Create new user (returns generated ID)
        /// Used for auto-registration during phone login
        /// </summary>
        Task<Guid> CreateAsync(User user, CancellationToken ct = default);

        /// <summary>
        /// Update existing user (e.g., update last_login_at, phone_verified_at)
        /// </summary>
        Task<bool> UpdateAsync(User user, CancellationToken ct = default);

        /// <summary>
        /// Get user with loaded role relationships (for generating JWT claims)
        /// Uses EF Core Include to eager load UserRoles and Roles
        /// </summary>
        Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken ct = default);
        Task<string?> GetUserFullNameAsync(Guid userId, CancellationToken ct = default);
        Task<string?> GetAvatarUrlAsync(Guid userId, CancellationToken ct = default);

        /// <summary>
        /// Get list of users with filtering and pagination
        /// </summary>
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(
            string? searchTerm,
            string? role,
            string? status,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

    }
}






