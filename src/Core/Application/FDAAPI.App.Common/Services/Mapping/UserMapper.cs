using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper implementation for User entity to UserDto
    /// </summary>
    public class UserMapper : IUserMapper
    {
        public UserDto MapToDto(User user)
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

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Roles = roleCodes
            };
        }

        public List<UserDto> MapToDtoList(IEnumerable<User> users)
        {
            if (users == null)
            {
                throw new ArgumentNullException(nameof(users));
            }

            return users.Select(MapToDto).ToList();
        }
    }
}

