using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG6;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat6.DTOs;
using MediatR;

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
        private readonly IMediator _mediator;

        public SendOtpEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/auth/send-otp");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Send OTP to phone number";
                s.Description = "Sends a one-time password to the provided phone number for authentication. " +
                               "OTP is valid for 5 minutes. " +
                               "In development, OTP is returned in response. " +
                               "In production, OTP is sent via SMS.";
                s.ExampleRequest = new SendOtpRequestDto
                {
                    Identifier = "+84901234567 or user@email.com"
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
                var command = new SendOtpRequest(req.Identifier);

                var result = await _mediator.Send(command, ct);

                var responseDto = new SendOtpResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    OtpCode = result.OtpCode,
                    ExpiresAt = result.ExpiresAt,
                    IdentifierType = result.IdentifierType
                };

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
