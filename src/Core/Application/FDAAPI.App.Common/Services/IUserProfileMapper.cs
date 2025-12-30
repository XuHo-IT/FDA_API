using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.Common.Services
{
    /// <summary>
    /// Service for mapping User entity to UserProfileDto
    /// </summary>
    public interface IUserProfileMapper
    {
        /// <summary>
        /// Maps User entity to UserProfileDto
        /// </summary>
        /// <param name="user">User entity with loaded roles</param>
        /// <returns>UserProfileDto</returns>
        UserProfileDto MapToProfileDto(User user);
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

