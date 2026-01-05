using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG10_AuthChangePassword
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordRequest, ChangePasswordResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public ChangePasswordHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<ChangePasswordResponse> Handle(
            ChangePasswordRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Validate passwords match
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "New password and confirmation password do not match",
                        StatusCode = ChangePasswordResponseStatusCode.PasswordMismatch
                    };
                }

                // 2. Validate new password strength
                var passwordValidation = ValidatePasswordStrength(request.NewPassword);
                if (!passwordValidation.IsValid)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = passwordValidation.ErrorMessage,
                        StatusCode = ChangePasswordResponseStatusCode.NewPasswordInvalid
                    };
                }

                // 3. Get user from database
                var user = await _userRepository.GetByIdAsync(request.UserId, ct);
                if (user == null)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = ChangePasswordResponseStatusCode.UserNotFound
                    };
                }

                // 4. Verify user has existing password
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "No existing password found. Please use set password instead",
                        StatusCode = ChangePasswordResponseStatusCode.CurrentPasswordIncorrect
                    };
                }

                // 5. Verify current password
                if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "Current password is incorrect",
                        StatusCode = ChangePasswordResponseStatusCode.CurrentPasswordIncorrect
                    };
                }

                // 6. Check if new password is same as current
                if (_passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash))
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "New password must be different from current password",
                        StatusCode = ChangePasswordResponseStatusCode.SameAsCurrentPassword
                    };
                }

                // 7. Hash new password
                var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

                // 8. Update user password
                user.PasswordHash = newPasswordHash;
                user.UpdatedBy = request.UserId;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userRepository.UpdateAsync(user, ct);
                if (!updateResult)
                {
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        Message = "Failed to update password",
                        StatusCode = ChangePasswordResponseStatusCode.UnknownError
                    };
                }

                // 9. Invalidate all refresh tokens (force re-login for security)
                await _refreshTokenRepository.RevokeAllUserTokensAsync(request.UserId, ct);

                return new ChangePasswordResponse
                {
                    Success = true,
                    Message = "Password changed successfully. Please log in again with your new password.",
                    StatusCode = ChangePasswordResponseStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = ChangePasswordResponseStatusCode.UnknownError
                };
            }
        }

        private (bool IsValid, string ErrorMessage) ValidatePasswordStrength(string password)
        {
            // Minimum 8 characters
            if (password.Length < 8)
            {
                return (false, "Password must be at least 8 characters long");
            }

            // Maximum 128 characters (prevent DoS)
            if (password.Length > 128)
            {
                return (false, "Password must not exceed 128 characters");
            }

            // Require at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "Password must contain at least one uppercase letter");
            }

            // Require at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return (false, "Password must contain at least one lowercase letter");
            }

            // Require at least one digit
            if (!Regex.IsMatch(password, @"\d"))
            {
                return (false, "Password must contain at least one digit");
            }

            // Require at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
            {
                return (false, "Password must contain at least one special character (!@#$%^&*(),.?\"':{}|<>)");
            }

            return (true, string.Empty);
        }
    }
}






