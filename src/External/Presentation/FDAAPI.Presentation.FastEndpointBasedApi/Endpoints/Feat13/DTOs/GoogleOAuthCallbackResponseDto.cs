namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat13.DTOs
{
    public class GoogleOAuthCallbackResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// FDA API JWT access token (60 minutes expiration)
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// FDA API refresh token (7 days expiration)
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public UserDto? User { get; set; }
        public string? ReturnUrl { get; set; }
        
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
