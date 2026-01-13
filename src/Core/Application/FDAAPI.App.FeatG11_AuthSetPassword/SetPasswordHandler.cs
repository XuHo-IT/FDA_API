using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Auth;
using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG11_AuthSetPassword
{
    public class SetPasswordHandler : IRequestHandler<SetPasswordRequest, SetPasswordResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public SetPasswordHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<SetPasswordResponse> Handle(
            SetPasswordRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Validate passwords match
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new SetPasswordResponse
                    {
                        Success = false,
                        Message = "New password and confirmation password do not match",
                        StatusCode = SetPasswordResponseStatusCode.PasswordMismatch
                    };
                }

                // 2. Validate new password strength
                var passwordValidation = ValidatePasswordStrength(request.NewPassword);
                if (!passwordValidation.IsValid)
                {
                    return new SetPasswordResponse
                    {
                        Success = false,
                        Message = passwordValidation.ErrorMessage,
                        StatusCode = SetPasswordResponseStatusCode.NewPasswordInvalid
                    };
                }

                // 3. Get user from database
                var user = await _userRepository.GetByIdAsync(request.UserId, ct);
                if (user == null)
                {
                    return new SetPasswordResponse
                    {
                        Success = false,
                        Message = "User not found",
                        StatusCode = SetPasswordResponseStatusCode.UserNotFound
                    };
                }

                // 4. Check if user already has password
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    return new SetPasswordResponse
                    {
                        Success = false,
                        Message = "Password already exists. Use change password instead",
                        StatusCode = SetPasswordResponseStatusCode.PasswordAlreadyExists
                    };
                }

                // 5. Validate and update email if provided
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    // Validate email format
                    if (!IsValidEmail(request.Email))
                    {
                        return new SetPasswordResponse
                        {
                            Success = false,
                            Message = "Invalid email format",
                            StatusCode = SetPasswordResponseStatusCode.EmailInvalid
                        };
                    }

                    // Check if email already exists (excluding current user)
                    var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
                    if (existingUser != null && existingUser.Id != request.UserId)
                    {
                        return new SetPasswordResponse
                        {
                            Success = false,
                            Message = "Email already exists",
                            StatusCode = SetPasswordResponseStatusCode.EmailAlreadyExists
                        };
                    }

                    user.Email = request.Email;
                    user.EmailVerifiedAt = null;  // Require email verification
                }

                // 6. Hash new password
                var passwordHash = _passwordHasher.HashPassword(request.NewPassword);

                // 7. Update user password and metadata
                user.PasswordHash = passwordHash;
                user.UpdatedBy = request.UserId;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userRepository.UpdateAsync(user, ct);
                if (!updateResult)
                {
                    return new SetPasswordResponse
                    {
                        Success = false,
                        Message = "Failed to set password",
                        StatusCode = SetPasswordResponseStatusCode.UnknownError
                    };
                }

                return new SetPasswordResponse
                {
                    Success = true,
                    Message = "Password set successfully. You can now log in with email and password.",
                    StatusCode = SetPasswordResponseStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new SetPasswordResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = SetPasswordResponseStatusCode.UnknownError
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

        private bool IsValidEmail(string email)
        {
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailRegex);
        }
    }
}






