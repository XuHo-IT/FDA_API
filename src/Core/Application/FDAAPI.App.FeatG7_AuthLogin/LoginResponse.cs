using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG7_AuthLogin
{
    /// <summary>
    /// Response from login operation
    /// </summary>
    public class LoginResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public UserDto? User { get; set; }
    }

    /// <summary>
    /// User DTO for login response (no sensitive data)
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}






