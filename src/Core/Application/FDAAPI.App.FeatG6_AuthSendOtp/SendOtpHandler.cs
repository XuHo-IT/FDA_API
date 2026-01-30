using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG6_AuthSendOtp
{
    /// <summary>
    /// Handler for sending OTP to phone number
    /// Generates mock OTP for development (returns in response)
    /// Production: Should integrate with SMS provider (Twilio, AWS SNS)
    /// </summary>
    public class SendOtpHandler : IRequestHandler<SendOtpRequest, SendOtpResponse>
    {
        private readonly IOtpCodeRepository _otpRepository;
        private readonly IOtpSender _otpSender;

        public SendOtpHandler(IOtpCodeRepository otpRepository, IOtpSender otpSender)
        {
            _otpRepository = otpRepository;
            _otpSender = otpSender;
        }

        public async Task<SendOtpResponse> Handle(SendOtpRequest request, CancellationToken ct)
        {
            // Validation: Identifier is required
            if (string.IsNullOrWhiteSpace(request.Identifier))
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = "Phone number or email is required"
                };
            }

            // Determine identifier type
            var identifierType = IsEmail(request.Identifier) ? "email" : "phone";

            // Basic validation
            if (identifierType == "phone" && request.Identifier.Length < 10)
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = "Invalid phone number format"
                };
            }

            if (identifierType == "email" && !IsEmail(request.Identifier))
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = "Invalid email format"
                };
            }

            try
            {
                // Generate 6-digit OTP (Mock for development)
                var otpCode = GenerateMockOtp();
                var expiresAt = DateTime.UtcNow.AddMinutes(5);

                // Save OTP to database
                var otp = new OtpCode
                {
                    Id = Guid.NewGuid(),
                    Identifier = request.Identifier,
                    IdentifierType = identifierType,
                    PhoneNumber = identifierType == "phone" ? request.Identifier : string.Empty, // Backward compatibility
                    Code = otpCode,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Empty, // System generated
                    IsUsed = false,
                    AttemptCount = 0
                };

                await _otpRepository.CreateAsync(otp, ct);

                // TODO: Production - Send OTP via SMS service
                //if (identifierType == "phone")
                //{
                //    // await _smsService.SendOtpAsync(request.Identifier, otpCode);
                //}
                //else
                //{
                //    // await _emailService.SendOtpAsync(request.Identifier, otpCode);
                //}

                // Send OTP via SMS or Email         
                var sent = await _otpSender.SendOtpAsync(
                    request.Identifier,
                    identifierType,
                    otpCode,
                    ct);

                if (!sent)
                {
                    return new SendOtpResponse
                    {
                        Success = false,
                        Message = $"Failed to send OTP to your {identifierType}. Please try again."
                    };
                }


                return new SendOtpResponse
                {
                    Success = true,
                    Message = $"OTP sent successfully to your {identifierType}",
                    OtpCode = otpCode, // Remove in production
                    ExpiresAt = expiresAt,
                    IdentifierType = identifierType
                };
            }
            catch (Exception ex)
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = $"Error sending OTP: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Generate mock OTP for development
        /// Production: Use Random.Shared.Next(100000, 999999).ToString()
        /// </summary>
        private string GenerateMockOtp()
        {
            // For development: return fixed OTP "123456"
            // return "123456";

            // For production: uncomment below
            return Random.Shared.Next(100000, 999999).ToString();
        }

        /// <summary>
        /// Simple email validation using regex
        /// </summary>
        private bool IsEmail(string identifier)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(identifier, emailPattern);
        }
    }
}






