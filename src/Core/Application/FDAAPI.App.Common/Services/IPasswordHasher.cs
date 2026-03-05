using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Services
{
    /// <summary>
    /// Service for hashing and verifying passwords using PBKDF2
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hash password using PBKDF2 with random salt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Base64-encoded hash with embedded salt</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verify password against stored hash (timing-safe comparison)
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="passwordHash">Stored password hash</param>
        /// <returns>True if password matches, false otherwise</returns>
        bool VerifyPassword(string password, string passwordHash);
    }
}






