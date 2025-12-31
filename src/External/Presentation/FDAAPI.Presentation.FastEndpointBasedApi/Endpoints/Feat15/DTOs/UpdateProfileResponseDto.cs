namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat15.DTOs
{
    /// <summary>
    /// Data Transfer Object for UpdateProfile response
    /// </summary>
    public class UpdateProfileResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserProfileDto? Profile { get; set; }
    }

    /// <summary>
    /// User profile DTO (public profile data only)
    /// </summary>
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public DateTime? PhoneVerifiedAt { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

