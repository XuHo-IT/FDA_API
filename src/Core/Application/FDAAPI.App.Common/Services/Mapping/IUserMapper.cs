using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.Common.Services.Mapping
{
    /// <summary>
    /// Mapper interface for User entity to UserDto
    /// </summary>
    public interface IUserMapper
    {
        /// <summary>
        /// Maps a User entity to UserDto (without sensitive data)
        /// </summary>
        /// <param name="user">The user entity</param>
        /// <returns>UserDto</returns>
        UserDto MapToDto(User user);

        /// <summary>
        /// Maps a collection of User entities to UserDto list
        /// </summary>
        /// <param name="users">Collection of user entities</param>
        /// <returns>List of UserDto</returns>
        List<UserDto> MapToDtoList(IEnumerable<User> users);
    }
}

