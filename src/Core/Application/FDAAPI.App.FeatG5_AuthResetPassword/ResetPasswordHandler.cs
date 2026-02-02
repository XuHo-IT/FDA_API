using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FDAAPI.App.Common.Models.Auth;
using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG5_AuthResetPassword
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordRequest, ResetPasswordResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public ResetPasswordHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<ResetPasswordResponse> Handle(
            ResetPasswordRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Validate passwords match
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "New password and confirmation password do not match",
                        StatusCode = ResetPasswordResponseStatusCode.PasswordMismatch
                    };
                }

                // 2. Validate password strength
                var passwordValidation = ValidatePasswordStrength(request.NewPassword);
                if (!passwordValidation.IsValid)
                {
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = passwordValidation.ErrorMessage,
                        StatusCode = ResetPasswordResponseStatusCode.NewPasswordInvalid
                    };
                }

                // 3. Get user from database
                var user = await _userRepository.GetByIdAsync(request.UserId, ct);
                if (user == null)
                {
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = ResetPasswordResponseStatusCode.UserNotFound
                    };
                }

                // 4. Check new password is different from current (if password exists)
                if (!string.IsNullOrEmpty(user.PasswordHash) &&
                    _passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash))
                {
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "New password must be different from current password",
                        StatusCode = ResetPasswordResponseStatusCode.SameAsCurrentPassword
                    };
                }

                // 5. Hash and update password
                user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                user.UpdatedBy = request.UserId;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userRepository.UpdateAsync(user, ct);
                if (!updateResult)
                {
                    return new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "Failed to reset password",
                        StatusCode = ResetPasswordResponseStatusCode.UnknownError
                    };
                }

                // 6. Revoke all refresh tokens (force re-login)
                await _refreshTokenRepository.RevokeAllUserTokensAsync(request.UserId, ct);

                return new ResetPasswordResponse
                {
                    Success = true,
                    Message = "Password reset successfully. Please log in again with your new password.",
                    StatusCode = ResetPasswordResponseStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = ResetPasswordResponseStatusCode.UnknownError
                };
            }
        }

        private (bool IsValid, string ErrorMessage) ValidatePasswordStrength(string password)
        {
            if (password.Length < 8)
                return (false, "Password must be at least 8 characters long");
            if (password.Length > 128)
                return (false, "Password must not exceed 128 characters");
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return (false, "Password must contain at least one uppercase letter");
            if (!Regex.IsMatch(password, @"[a-z]"))
                return (false, "Password must contain at least one lowercase letter");
            if (!Regex.IsMatch(password, @"\d"))
                return (false, "Password must contain at least one digit");
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
                return (false, "Password must contain at least one special character (!@#$%^&*(),.?\"':{}|<>)");
            return (true, string.Empty);
        }
    }
}
