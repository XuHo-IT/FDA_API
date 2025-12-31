using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services.IServices;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;

namespace FDAAPI.App.FeatG7
{
    /// <summary>
    /// Handler for user login
    /// Supports dual authentication:
    /// 1. Phone + OTP (auto-register if not exists)
    /// 2. Email + Password (must exist)
    /// </summary>
    public class LoginHandler : IFeatureHandler<LoginRequest, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public LoginHandler(
            IUserRepository userRepository,
            IOtpCodeRepository otpRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken ct)
        {
            // Determine login method
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !string.IsNullOrWhiteSpace(request.OtpCode))
            {
                return await HandlePhoneOtpLoginAsync(request, ct);
            }
            else if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Password))
            {
                return await HandleEmailPasswordLoginAsync(request, ct);
            }

            return new LoginResponse
            {
                Success = false,
                Message = "Invalid login credentials format. Provide either (phone + OTP) or (email + password)."
            };
        }

        /// <summary>
        /// Handle Phone + OTP login flow (Citizens)
        /// Auto-registers user if not exists
        /// </summary>
        private async Task<LoginResponse> HandlePhoneOtpLoginAsync(LoginRequest request, CancellationToken ct)
        {
            try
            {
                // Step 1: Verify OTP
                var otp = await _otpRepository.GetLatestValidOtpAsync(request.PhoneNumber!, ct);

                if (otp == null || otp.IsUsed || otp.ExpiresAt < DateTime.UtcNow)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid or expired OTP. Please request a new OTP."
                    };
                }

                if (otp.Code != request.OtpCode)
                {
                    // Increment attempt count
                    await _otpRepository.IncrementAttemptCountAsync(otp.Id, ct);

                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Incorrect OTP code. Please try again."
                    };
                }

                // Step 2: Check if user exists, if not create new user (AUTO-REGISTRATION)
                var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber!, ct);

                if (user == null)
                {
                    // Auto-register new citizen user
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        PhoneNumber = request.PhoneNumber,
                        Email = $"{request.PhoneNumber}@temp.fda.local", // Temporary email
                        Provider = "local",
                        Status = "active",
                        PhoneVerifiedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = Guid.Empty
                    };

                    await _userRepository.CreateAsync(user, ct);

                    // Assign USER role
                    var userRole = await _roleRepository.GetByCodeAsync("USER", ct);
                    if (userRole != null)
                    {
                        await _userRoleRepository.AssignRoleToUserAsync(user.Id, userRole.Id, ct);
                    }
                }
                else
                {
                    // Update phone verification timestamp
                    user.PhoneVerifiedAt = DateTime.UtcNow;
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user, ct);
                }

                // Step 3: Mark OTP as used
                await _otpRepository.MarkAsUsedAsync(otp.Id, ct);

                // Step 4: Generate tokens
                return await GenerateTokenResponseAsync(user, request, ct);
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Login error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Handle Email + Password login flow (Admin/Gov)
        /// User must exist (no auto-registration)
        /// </summary>
        private async Task<LoginResponse> HandleEmailPasswordLoginAsync(LoginRequest request, CancellationToken ct)
        {
            try
            {
                // Step 1: Find user by email
                var user = await _userRepository.GetByEmailAsync(request.Email!, ct);

                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Step 2: Verify password
                if (string.IsNullOrEmpty(user.PasswordHash) ||
                    !_passwordHasher.VerifyPassword(request.Password!, user.PasswordHash))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Step 3: Check if user is banned
                if (user.Status == "banned")
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Your account has been banned. Please contact administrator."
                    };
                }

                // Step 4: Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user, ct);

                // Step 5: Generate tokens
                return await GenerateTokenResponseAsync(user, request, ct);
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Login error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Generate JWT tokens and build login response
        /// </summary>
        private async Task<LoginResponse> GenerateTokenResponseAsync(
            User user,
            LoginRequest request,
            CancellationToken ct)
        {
            // Get user roles
            var roles = await _userRoleRepository.GetUserRolesAsync(user.Id, ct);
            var roleCodes = roles.Select(r => r.Code).ToList();

            // Generate JWT access token
            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, roleCodes);

            // Generate refresh token
            var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id,
                DeviceInfo = request.DeviceInfo,
                IpAddress = request.IpAddress,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(refreshToken, ct);

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Access token expiry
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Roles = roleCodes
                }
            };
        }
    }
}
