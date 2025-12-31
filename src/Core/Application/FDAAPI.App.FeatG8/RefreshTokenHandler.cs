using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.IServices;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;

namespace FDAAPI.App.FeatG8
{
    /// <summary>
    /// Handler for refreshing access token
    /// Implements token rotation: old token is revoked, new token is issued
    /// Security: Prevents refresh token reuse attacks
    /// </summary>
    public class RefreshTokenHandler : IFeatureHandler<RefreshTokenRequest, RefreshTokenResponse>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public RefreshTokenHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IJwtTokenService jwtTokenService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<RefreshTokenResponse> ExecuteAsync(
            RefreshTokenRequest request,
            CancellationToken ct)
        {
            // Validation: Refresh token is required
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Refresh token is required"
                };
            }

            try
            {
                // Step 1: Find refresh token in database
                var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, ct);

                if (token == null)
                {
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    };
                }

                // Step 2: Check if token is revoked (prevent token reuse)
                if (token.IsRevoked)
                {
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Refresh token has been revoked. Please login again."
                    };
                }

                // Step 3: Check if token is expired
                if (token.ExpiresAt < DateTime.UtcNow)
                {
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Refresh token has expired. Please login again."
                    };
                }

                // Step 4: Get user from token
                var user = await _userRepository.GetByIdAsync(token.UserId, ct);

                if (user == null)
                {
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Step 5: Check if user is still active
                if (user.Status == "banned")
                {
                    return new RefreshTokenResponse
                    {
                        Success = false,
                        Message = "Your account has been banned"
                    };
                }

                // Step 6: Get user roles for JWT claims
                var roles = await _userRoleRepository.GetUserRolesAsync(user.Id, ct);
                var roleCodes = roles.Select(r => r.Code).ToList();

                // Step 7: Generate NEW access token
                var newAccessToken = _jwtTokenService.GenerateAccessToken(
                    user.Id,
                    user.Email,
                    roleCodes
                );

                // Step 8: Generate NEW refresh token (token rotation)
                var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
                var newRefreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = newRefreshTokenValue,
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = user.Id,
                    DeviceInfo = token.DeviceInfo, // Keep same device info
                    IpAddress = token.IpAddress,   // Keep same IP
                    IsRevoked = false
                };

                // Step 9: Revoke OLD refresh token (security: prevent reuse)
                await _refreshTokenRepository.RevokeTokenAsync(request.RefreshToken, ct);

                // Step 10: Save NEW refresh token to database
                await _refreshTokenRepository.CreateAsync(newRefreshToken, ct);

                // Step 11: Return new tokens
                return new RefreshTokenResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshTokenValue,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Access token expiry
                };
            }
            catch (Exception ex)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = $"Error refreshing token: {ex.Message}"
                };
            }
        }
    }
}
