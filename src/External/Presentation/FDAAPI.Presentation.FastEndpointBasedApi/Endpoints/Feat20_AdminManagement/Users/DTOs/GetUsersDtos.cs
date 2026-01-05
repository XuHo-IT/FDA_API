using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_AdminManagement.Users.DTOs
{
    public class GetUsersRequestDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetUsersResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<UserProfileDto> Users { get; set; } = new List<UserProfileDto>();
        public int TotalCount { get; set; }
    }
}










