using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.Infra.Services.Mapping
{
    /// <summary>
    /// Implementation of IUserProfileMapper
    /// Maps User entity to UserProfileDto
    /// </summary>
    public class UserProfileMapper : IUserProfileMapper
    {
        public UserProfileDto MapToProfileDto(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Extract role codes from loaded navigation properties
            var roleCodes = user.UserRoles
                .Select(ur => ur.Role?.Code ?? string.Empty)
                .Where(code => !string.IsNullOrEmpty(code))
                .ToList();

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Provider = user.Provider,
                Status = user.Status,
                LastLoginAt = user.LastLoginAt,
                PhoneVerifiedAt = user.PhoneVerifiedAt,
                EmailVerifiedAt = user.EmailVerifiedAt,
                Roles = roleCodes,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}

