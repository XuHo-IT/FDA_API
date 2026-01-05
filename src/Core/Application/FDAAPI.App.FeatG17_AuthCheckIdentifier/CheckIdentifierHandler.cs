using FDAAPI.App.Common.Features;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG17_AuthCheckIdentifier
{
    /// <summary>
    /// Handler to check identifier and determine authentication method
    /// </summary>
    public class CheckIdentifierHandler : IRequestHandler<CheckIdentifierRequest, CheckIdentifierResponse>
    {
        private readonly IUserRepository _userRepository;

        public CheckIdentifierHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<CheckIdentifierResponse> Handle(
            CheckIdentifierRequest request,
            CancellationToken ct = default)
        {
            try
            {
                // Validate identifier
                if (string.IsNullOrWhiteSpace(request.Identifier))
                {
                    return new CheckIdentifierResponse
                    {
                        Success = false,
                        Message = "Identifier is required"
                    };
                }

                // Determine identifier type
                var identifierType = IsEmail(request.Identifier) ? "email" : "phone";

                // Find user by identifier
                var user = identifierType == "email"
                    ? await _userRepository.GetByEmailAsync(request.Identifier, ct)
                    : await _userRepository.GetByPhoneNumberAsync(request.Identifier, ct);

                // Case 1: Account does not exist
                if (user == null)
                {
                    return new CheckIdentifierResponse
                    {
                        Success = true,
                        Message = $"No account found. OTP will be sent to verify your {identifierType}.",
                        IdentifierType = identifierType,
                        AccountExists = false,
                        HasPassword = false,
                        RequiredMethod = "otp"
                    };
                }

                // Case 2: Account exists but no password
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return new CheckIdentifierResponse
                    {
                        Success = true,
                        Message = $"Account found without password. OTP will be sent to your {identifierType}.",
                        IdentifierType = identifierType,
                        AccountExists = true,
                        HasPassword = false,
                        RequiredMethod = "otp"
                    };
                }

                // Case 3: Account exists with password
                return new CheckIdentifierResponse
                {
                    Success = true,
                    Message = "Please enter your password",
                    IdentifierType = identifierType,
                    AccountExists = true,
                    HasPassword = true,
                    RequiredMethod = "password"
                };
            }
            catch (Exception ex)
            {
                return new CheckIdentifierResponse
                {
                    Success = false,
                    Message = $"Error checking identifier: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Simple email validation
        /// </summary>
        private bool IsEmail(string identifier)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(identifier, emailPattern);
        }
    }
}






