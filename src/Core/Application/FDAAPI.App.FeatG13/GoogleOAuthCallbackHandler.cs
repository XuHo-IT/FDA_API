using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.IServices;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Infra.Services.Cache;
using FDAAPI.Infra.Services.OAuth;

namespace FDAAPI.App.FeatG13
{
    public class GoogleOAuthCallbackHandler : IFeatureHandler<GoogleOAuthCallbackRequest, GoogleOAuthCallbackResponse>
    {
        private readonly IGoogleOAuthService _googleOAuthService;
        private readonly IStateCache _stateCache;
        private readonly IUserRepository _userRepository;
        private readonly IUserOAuthProviderRepository _oauthProviderRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public GoogleOAuthCallbackHandler(
            IGoogleOAuthService googleOAuthService,
            IStateCache stateCache,
            IUserRepository userRepository,
            IUserOAuthProviderRepository oauthProviderRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenService jwtTokenService)
        {
            _googleOAuthService = googleOAuthService;
            _stateCache = stateCache;
            _userRepository = userRepository;
            _oauthProviderRepository = oauthProviderRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<GoogleOAuthCallbackResponse> ExecuteAsync(
            GoogleOAuthCallbackRequest request,
            CancellationToken ct = default)
        {
            try
            {
                // 1. Validate state token (CSRF protection)
                var cachedReturnUrl = await _stateCache.GetStateAsync(request.State, ct);
                if (cachedReturnUrl == null)
                {
                    return new GoogleOAuthCallbackResponse
                    {
                        Success = false,
                        Message = "Invalid or expired state token"
                    };
                }

                // Remove state after validation (one-time use)
                await _stateCache.RemoveStateAsync(request.State, ct);

                // 2. Exchange authorization code for tokens
                GoogleTokenResponse tokenResponse;
                try
                {
                    tokenResponse = await _googleOAuthService.ExchangeCodeForTokenAsync(request.Code, ct);
                }
                catch (HttpRequestException ex)
                {
                    return new GoogleOAuthCallbackResponse
                    {
                        Success = false,
                        Message = "Failed to exchange authorization code with Google"
                    };
                }

                // 3. Verify ID token and get user info
                GoogleUserInfo googleUser;
                try
                {
                    googleUser = await _googleOAuthService.VerifyIdTokenAsync(tokenResponse.IdToken, ct);
                }
                catch (Exception ex)
                {
                    return new GoogleOAuthCallbackResponse
                    {
                        Success = false,
                        Message = "Failed to verify Google ID token"
                    };
                }

                // 4. Check if OAuth provider record exists
                var oauthProvider = await _oauthProviderRepository
                    .GetByProviderUserIdAsync("google", googleUser.Id, ct);

                User user;

                if (oauthProvider != null)
                {
                    // Scenario A: Existing user (Re-login)
                    user = oauthProvider.User;

                    // Update OAuth provider info
                    oauthProvider.DisplayName = googleUser.Name;
                    oauthProvider.ProfilePictureUrl = googleUser.Picture;
                    oauthProvider.LastLoginAt = DateTime.UtcNow;
                    oauthProvider.UpdatedAt = DateTime.UtcNow;
                    oauthProvider.UpdatedBy = user.Id;

                    await _oauthProviderRepository.UpdateAsync(oauthProvider, ct);

                    // Avatar sync: Only update if user has NO avatar
                    if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(googleUser.Picture))
                    {
                        user.AvatarUrl = googleUser.Picture;
                        user.UpdatedAt = DateTime.UtcNow;
                        user.UpdatedBy = user.Id;
                        await _userRepository.UpdateAsync(user, ct);
                    }
                }
                else
                {
                    // Scenario B: New user (First login) OR Account linking

                    // Check if user exists by email
                    var existingUser = await _userRepository.GetByEmailAsync(googleUser.Email, ct);

                    if (existingUser != null)
                    {
                        // Account linking: User exists with email/password
                        user = existingUser;
                    }
                    else
                    {
                        // New user: Create account
                        user = new User
                        {
                            Id = Guid.NewGuid(),
                            Email = googleUser.Email,
                            FullName = googleUser.Name,
                            AvatarUrl = googleUser.Picture,
                            Provider = "google",
                            Status = "ACTIVE",
                            EmailVerifiedAt = googleUser.EmailVerified ? DateTime.UtcNow : null,
                            CreatedBy = Guid.Empty, // System
                            CreatedAt = DateTime.UtcNow,
                            UpdatedBy = Guid.Empty,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _userRepository.CreateAsync(user, ct);

                        // Assign USER (Citizen) role
                        var userRole = await _roleRepository.GetByCodeAsync("USER", ct);
                        if (userRole == null)
                        {
                            return new GoogleOAuthCallbackResponse
                            {
                                Success = false,
                                Message = "USER role not found in database"
                            };
                        }

                        await _userRoleRepository.AssignRoleToUserAsync(user.Id, userRole.Id, ct);

                    }

                    // Create OAuth provider record
                    await _oauthProviderRepository.CreateAsync(new UserOAuthProvider
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Provider = "google",
                        ProviderUserId = googleUser.Id,
                        Email = googleUser.Email,
                        DisplayName = googleUser.Name,
                        ProfilePictureUrl = googleUser.Picture,
                        LastLoginAt = DateTime.UtcNow,
                        CreatedBy = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = user.Id,
                        UpdatedAt = DateTime.UtcNow
                    }, ct);
                }

                // 5. Load user roles
                var roles = await _userRoleRepository.GetUserRolesAsync(user.Id, ct);
                var roleCodes = roles.Select(r => r.Code).ToList();

                // 6. Generate JWT tokens (same as LoginHandler)
                var accessToken = _jwtTokenService.GenerateAccessToken(
                    user.Id,
                    user.Email,
                    roleCodes
                );

                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                // 7. Store refresh token
                await _refreshTokenRepository.CreateAsync(new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = refreshTokenExpiry,
                    IsRevoked = false,
                    CreatedBy = user.Id,
                    CreatedAt = DateTime.UtcNow
                }, ct);

                // 8. Update LastLoginAt
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = user.Id;
                await _userRepository.UpdateAsync(user, ct);

                // 9. Return success response
                return new GoogleOAuthCallbackResponse
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Access token expiry
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        AvatarUrl = user.AvatarUrl,
                        Roles = roleCodes
                    }
                };
            }
            catch (Exception ex)
            {
                return new GoogleOAuthCallbackResponse
                {
                    Success = false,
                    Message = $"OAuth callback error: {ex.Message}"
                };
            }
        }
    }
}
