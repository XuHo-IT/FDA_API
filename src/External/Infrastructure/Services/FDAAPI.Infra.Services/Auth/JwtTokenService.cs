using FDAAPI.App.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FDAAPI.Infra.Services.Auth
{
    /// <summary>
    /// JWT Token Service implementation using System.IdentityModel.Tokens.Jwt
    /// Generates access tokens (short-lived) and refresh tokens (long-lived)
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generate JWT access token with user claims and role-based authorization
        /// </summary>
        public string GenerateAccessToken(Guid userId, string email, List<string> roles)
        {
            // Get JWT configuration from appsettings.json
            var secret = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret not configured in appsettings.json");
            var issuer = _configuration["Jwt:Issuer"] ?? "FDA_API";
            var audience = _configuration["Jwt:Audience"] ?? "FDA_Clients";
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

            // Create security key from secret
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Build claims list
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add role claims for authorization
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create JWT token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            // Serialize token to string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generate cryptographically secure random refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            // Generate 64 random bytes
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            // Convert to Base64 string
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Validate JWT token and extract user ID (with signature verification)
        /// </summary>
        public Guid? ValidateAccessToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret not configured");
            var key = Encoding.UTF8.GetBytes(secret);

            try
            {
                // Validate token with strict parameters
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                }, out SecurityToken validatedToken);

                // Extract user ID from claims
                var jwtToken = (JwtSecurityToken)validatedToken;
                var userIdClaim = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

                return Guid.Parse(userIdClaim);
            }
            catch (Exception)
            {
                // Token validation failed (expired, invalid signature, etc.)
                return null;
            }
        }
    }
}






