using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG6;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat6.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat6
{
    /// <summary>
    /// Endpoint for sending OTP to phone number
    /// 
    /// Request Flow:
    ///   1. Client sends POST request to /api/v1/auth/send-otp
    ///   2. FastEndpoint deserializes request DTO
    ///   3. Endpoint maps DTO to SendOtpRequest
    ///   4. Handler generates and saves OTP
    ///   5. Endpoint returns OTP (dev) or success message (prod)
    /// </summary>
    public class SendOtpEndpoint : Endpoint<SendOtpRequestDto, SendOtpResponseDto>
    {
        private readonly IFeatureHandler<SendOtpRequest, SendOtpResponse> _handler;

        public SendOtpEndpoint(IFeatureHandler<SendOtpRequest, SendOtpResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            // Define HTTP method and route
            Post("/api/v1/auth/send-otp");

            // Allow anonymous access (no authentication required)
            AllowAnonymous();

            // API documentation
            Summary(s =>
            {
                s.Summary = "Send OTP to phone number";
                s.Description = "Sends a one-time password to the provided phone number for authentication. " +
                               "OTP is valid for 5 minutes. " +
                               "In development, OTP is returned in response. " +
                               "In production, OTP is sent via SMS.";
                s.ExampleRequest = new SendOtpRequestDto
                {
                    PhoneNumber = "+84901234567"
                };
                s.ResponseExamples[200] = new SendOtpResponseDto
                {
                    Success = true,
                    Message = "OTP sent successfully",
                    OtpCode = "123456", // Dev only
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };
            });

            Tags("Authentication", "OTP");
        }

        public override async Task HandleAsync(SendOtpRequestDto req, CancellationToken ct)
        {
            try
            {
                // Step 1: Map DTO to application request
                var appRequest = new SendOtpRequest
                {
                    PhoneNumber = req.PhoneNumber
                };

                // Step 2: Execute handler
                var result = await _handler.ExecuteAsync(appRequest, ct);

                // Step 3: Map to response DTO
                var responseDto = new SendOtpResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    OtpCode = result.OtpCode, // Dev only - remove in production
                    ExpiresAt = result.ExpiresAt
                };

                // Step 4: Send response
                if (result.Success)
                {
                    await SendAsync(responseDto, 200, ct);
                }
                else
                {
                    await SendAsync(responseDto, 400, ct);
                }
            }
            catch (Exception ex)
            {
                var errorDto = new SendOtpResponseDto
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}
