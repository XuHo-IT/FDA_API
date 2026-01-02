using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Services.Mapping
{
    public interface IUserProfileMapper
    {
        /// <summary>
        /// Maps User entity to UserProfileDto
        /// </summary>
        /// <param name="user">User entity with loaded roles</param>
        /// <returns>UserProfileDto</returns>
        UserProfileDto MapToProfileDto(User user);
    }
}
