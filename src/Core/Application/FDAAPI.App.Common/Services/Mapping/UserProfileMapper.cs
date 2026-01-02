using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Services.Mapping
{
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
