using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.Common.Services.Mapping
{
    public interface IUserProfileMapper
    {
        UserProfileDto MapToProfileDto(User user);
    }
}

