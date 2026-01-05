namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat20_AdminManagement.Users.DTOs
{
    public class CreateUserRequestDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public List<string> RoleNames { get; set; } = new();
    }

    public class CreateUserResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
    }
}










