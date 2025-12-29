using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Features;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;

namespace FDAAPI.App.FeatG6
{
    /// <summary>
    /// Handler for sending OTP to phone number
    /// Generates mock OTP for development (returns in response)
    /// Production: Should integrate with SMS provider (Twilio, AWS SNS)
    /// </summary>
    public class SendOtpHandler : IFeatureHandler<SendOtpRequest, SendOtpResponse>
    {
        private readonly IOtpCodeRepository _otpRepository;

        public SendOtpHandler(IOtpCodeRepository otpRepository)
        {
            _otpRepository = otpRepository;
        }

        public async Task<SendOtpResponse> ExecuteAsync(SendOtpRequest request, CancellationToken ct)
        {
            // Validation: Phone number is required
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = "Phone number is required"
                };
            }

            // Basic phone validation (simple check)
            if (request.PhoneNumber.Length < 10)
            {
                return new SendOtpResponse
                {
                    Success = false,
                    Message = "Invalid phone number format"
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
                    PhoneNumber = request.PhoneNumber,
                    Code = otpCode,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Empty, // System generated
                    IsUsed = false,
                    AttemptCount = 0
                };

                await _otpRepository.CreateAsync(otp, ct);

                // TODO: Production - Send OTP via SMS service
                // await _smsService.SendOtpAsync(request.PhoneNumber, otpCode);

                return new SendOtpResponse
                {
                    Success = true,
                    Message = "OTP sent successfully",
                    OtpCode = otpCode, // Remove in production
                    ExpiresAt = expiresAt
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
            return "123456";

            // For production: uncomment below
            // return Random.Shared.Next(100000, 999999).ToString();
        }
    }
}
