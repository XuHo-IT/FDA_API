using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Services.IServices
{
    /// <summary>
    /// Service for generating and validating JWT tokens
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generate JWT access token with user claims
        /// </summary>
        /// <param name="userId">User unique identifier</param>
        /// <param name="email">User email address</param>
        /// <param name="roles">List of user roles</param>
        /// <returns>JWT access token string</returns>
        string GenerateAccessToken(Guid userId, string email, List<string> roles);

        /// <summary>
        /// Generate random refresh token
        /// </summary>
        /// <returns>Base64-encoded refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validate JWT access token and extract user ID
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>User ID if valid, null if invalid</returns>
        Guid? ValidateAccessToken(string token);
    }
}
