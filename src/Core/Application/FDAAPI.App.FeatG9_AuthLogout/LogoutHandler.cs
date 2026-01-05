using FDAAPI.App.Common.Features;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG9_AuthLogout
{
    /// <summary>
    /// Handler for user logout
    /// Supports two modes:
    /// 1. Single device logout (revoke specific refresh token)
    /// 2. All devices logout (revoke all user's refresh tokens)
    /// </summary>
    public class LogoutHandler : IRequestHandler<LogoutRequest, LogoutResponse>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LogoutHandler(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LogoutResponse> Handle(LogoutRequest request, CancellationToken ct)
        {
            // Validation: Refresh token is required
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return new LogoutResponse
                {
                    Success = false,
                    Message = "Refresh token is required",
                    TokensRevoked = 0
                };
            }

            try
            {
                if (request.RevokeAllTokens)
                {
                    // Mode 1: Logout from ALL devices
                    return await LogoutFromAllDevicesAsync(request.RefreshToken, ct);
                }
                else
                {
                    // Mode 2: Logout from CURRENT device only
                    return await LogoutFromCurrentDeviceAsync(request.RefreshToken, ct);
                }
            }
            catch (Exception ex)
            {
                return new LogoutResponse
                {
                    Success = false,
                    Message = $"Error during logout: {ex.Message}",
                    TokensRevoked = 0
                };
            }
        }

        /// <summary>
        /// Logout from current device only (revoke specific token)
        /// </summary>
        private async Task<LogoutResponse> LogoutFromCurrentDeviceAsync(
            string refreshToken,
            CancellationToken ct)
        {
            // Revoke the specific refresh token
            var revoked = await _refreshTokenRepository.RevokeTokenAsync(refreshToken, ct);

            if (!revoked)
            {
                return new LogoutResponse
                {
                    Success = false,
                    Message = "Refresh token not found or already revoked",
                    TokensRevoked = 0
                };
            }

            return new LogoutResponse
            {
                Success = true,
                Message = "Logout successful",
                TokensRevoked = 1
            };
        }

        /// <summary>
        /// Logout from ALL devices (revoke all user's tokens)
        /// Use case: "Logout from all devices" button, or security incident
        /// </summary>
        private async Task<LogoutResponse> LogoutFromAllDevicesAsync(
            string refreshToken,
            CancellationToken ct)
        {
            // Step 1: Get token to find user ID
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, ct);

            if (token == null)
            {
                return new LogoutResponse
                {
                    Success = false,
                    Message = "Refresh token not found",
                    TokensRevoked = 0
                };
            }

            // Step 2: Revoke ALL tokens for this user
            var revoked = await _refreshTokenRepository.RevokeAllUserTokensAsync(token.UserId, ct);

            if (!revoked)
            {
                return new LogoutResponse
                {
                    Success = false,
                    Message = "Failed to revoke tokens",
                    TokensRevoked = 0
                };
            }

            return new LogoutResponse
            {
                Success = true,
                Message = "Logged out from all devices successfully",
                TokensRevoked = -1 // -1 indicates "all tokens"
            };
        }
    }
}






