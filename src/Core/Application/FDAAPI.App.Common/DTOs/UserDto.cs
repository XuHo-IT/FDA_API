using System;
using System.Collections.Generic;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// User DTO for responses (no sensitive data)
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
