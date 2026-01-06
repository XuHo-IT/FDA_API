using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat21_UserList.DTOs
{
    public class GetUsersResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<UserProfileDto> Users { get; set; } = new List<UserProfileDto>();
        public int TotalCount { get; set; }
    }
}
