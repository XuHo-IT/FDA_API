using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG7_AuthLogin
{
    /// <summary>
    /// Handler for user login
    /// Supports dual authentication:
    /// 1. Phone + OTP (auto-register if not exists)
    /// 2. Email + Password (must exist)
    /// </summary>
    public class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IOtpCodeRepository _otpRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserMapper _userMapper;

        public LoginHandler(
            IUserRepository userRepository,
            IOtpCodeRepository otpRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IUserMapper userMapper)
        {
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _userMapper = userMapper;
        }

        public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken ct)
        {
            // Normalize identifier (priority: Identifier > PhoneNumber/Email)
            var identifier = request.Identifier
                ?? request.PhoneNumber
                ?? request.Email;

            if (string.IsNullOrWhiteSpace(identifier))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Identifier (phone or email) is required."
                };
            }

            // Determine authentication method
            if (!string.IsNullOrWhiteSpace(request.OtpCode))
            {
                return await HandleOtpLoginAsync(identifier, request.OtpCode, request, ct);
            }
            else if (!string.IsNullOrWhiteSpace(request.Password))
            {
                return await HandlePasswordLoginAsync(identifier, request.Password, request, ct);
            }

            return new LoginResponse
            {
                Success = false,
                Message = "Either OTP code or password is required."
            };
        }

        /// <summary>
        /// Handle OTP login flow (supports both phone and email)
        /// Auto-registers user if phone number doesn't exist
        /// For email: requires existing account (forgot password flow)
        /// </summary>
        /// <summary>
        /// Handle OTP login flow (supports both phone and email)
        /// Use cases:
        /// 1. New user registration (auto-register for both phone and email)
        /// 2. Login for accounts without password
        /// 3. Forgot password verification (accounts with password)
        /// </summary>
        private async Task<LoginResponse> HandleOtpLoginAsync(
            string identifier,
            string otpCode,
            LoginRequest request,
            CancellationToken ct)
        {
            try
            {
                // Step 1: Determine identifier type
                var isEmail = identifier.Contains("@");
                var identifierType = isEmail ? "email" : "phone";

                // Step 2: Verify OTP from database
                var otp = await _otpRepository.GetLatestValidOtpByIdentifierAsync(identifier, ct);

                if (otp == null || otp.IsUsed || otp.ExpiresAt < DateTime.UtcNow)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid or expired OTP. Please request a new OTP."
                    };
                }

                // Step 3: Verify OTP code matches
                if (otp.Code != otpCode)
                {
                    await _otpRepository.IncrementAttemptCountAsync(otp.Id, ct);
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Incorrect OTP code. Please try again."
                    };
                }

                // Step 4: Find user by identifier
                var user = isEmail
                    ? await _userRepository.GetByEmailAsync(identifier, ct)
                    : await _userRepository.GetByPhoneNumberAsync(identifier, ct);

                // Step 5: Handle based on user existence
                if (user == null)
                {
                    // USE CASE 1: New user registration (for both phone and email)
                    if (isEmail)
                    {
                        // Auto-register with email
                        user = new User
                        {
                            Id = Guid.NewGuid(),
                            Email = identifier,
                            PhoneNumber = null, // No phone number yet (avoid unique constraint violation)
                            Provider = "local",
                            Status = "active",
                            EmailVerifiedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Empty,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = Guid.Empty
                        };
                    }
                    else
                    {
                        // Auto-register with phone
                        user = new User
                        {
                            Id = Guid.NewGuid(),
                            PhoneNumber = identifier,
                            Email = $"{identifier}@temp.fda.local", // Temporary email
                            Provider = "local",
                            Status = "active",
                            PhoneVerifiedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = Guid.Empty,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = Guid.Empty
                        };
                    }

                    await _userRepository.CreateAsync(user, ct);

                    // Assign default USER role
                    var userRole = await _roleRepository.GetByCodeAsync("USER", ct);
                    if (userRole != null)
                    {
                        await _userRoleRepository.AssignRoleToUserAsync(user.Id, userRole.Id, ct);
                    }
                }
                else
                {
                    // USE CASE 2: Account exists WITHOUT password ? Allow OTP login
                    // USE CASE 3: Account exists WITH password ? Allow OTP (forgot password flow)

                    // Update verification timestamps
                    if (isEmail)
                    {
                        user.EmailVerifiedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        user.PhoneVerifiedAt = DateTime.UtcNow;
                    }

                    user.LastLoginAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user, ct);
                }

                // Step 6: Check if account is banned
                if (user.Status == "banned")
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Your account has been banned. Please contact administrator."
                    };
                }

                // Step 7: Mark OTP as used (prevent replay attacks)
                await _otpRepository.MarkAsUsedAsync(otp.Id, ct);

                // Step 8: Generate JWT tokens and return
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
        /// Handle Password login flow (supports both phone and email)
        /// User must exist (no auto-registration)
        /// User must have password set
        /// </summary>
        private async Task<LoginResponse> HandlePasswordLoginAsync(
            string identifier,
            string password,
            LoginRequest request,
            CancellationToken ct)
        {
            try
            {
                // Step 1: Determine identifier type
                var isEmail = identifier.Contains("@");
                var identifierType = isEmail ? "email" : "phone";

                // Step 2: Find user by identifier
                var user = isEmail
                    ? await _userRepository.GetByEmailAsync(identifier, ct)
                    : await _userRepository.GetByPhoneNumberAsync(identifier, ct);

                // Step 3: Check if user exists
                if (user == null)
                {
                    // Generic error message for security (don't reveal if user exists)
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    };
                }

                // Step 4: Verify user has password set
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = $"No password set for this account. Please use OTP login or set a password first."
                    };
                }

                // Step 5: Verify password matches
                if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    // Generic error message for security
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    };
                }

                // Step 6: Check if account is banned
                if (user.Status == "banned")
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Your account has been banned. Please contact administrator."
                    };
                }

                // Step 7: Check if account is inactive
                if (user.Status == "inactive")
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Your account is inactive. Please contact administrator."
                    };
                }

                // Step 8: Update last login timestamp
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user, ct);

                // Step 9: Generate JWT tokens and return
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

            // Load user with roles for mapping
            var userWithRoles = await _userRepository.GetUserWithRolesAsync(user.Id, ct);

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Access token expiry
                User = _userMapper.MapToDto(userWithRoles)
            };
        }
    }
}






