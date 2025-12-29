using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG11;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat11.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat11
{
    public class SetPasswordEndpoint : Endpoint<SetPasswordRequestDto, SetPasswordResponseDto>
    {
        private readonly IFeatureHandler<SetPasswordRequest, SetPasswordResponse> _handler;

        public SetPasswordEndpoint(IFeatureHandler<SetPasswordRequest, SetPasswordResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            Post("/api/v1/auth/set-password");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Summary(s =>
            {
                s.Summary = "Set password for first-time (phone-only users)";
                s.Description = "Allows phone-only users (registered via OTP) to set their first password. Can optionally update email for email-based login.";
                s.ExampleRequest = new SetPasswordRequestDto
                {
                    Email = "user@example.com",
                    NewPassword = "NewPassword123!",
                    ConfirmPassword = "NewPassword123!"
                };
            });
            Tags("Authentication", "Password Management");
        }

        public override async Task HandleAsync(SetPasswordRequestDto req, CancellationToken ct)
        {
            // Extract user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new SetPasswordResponseDto
                {
                    Success = false,
                    Message = "Invalid user authentication"
                }, 401, ct);
                return;
            }

            // Map DTO to Request
            var appRequest = new SetPasswordRequest
            {
                UserId = userId,
                Email = req.Email,
                NewPassword = req.NewPassword,
                ConfirmPassword = req.ConfirmPassword
            };

            // Execute handler
            var result = await _handler.ExecuteAsync(appRequest, ct);

            // Map Response to DTO
            var response = new SetPasswordResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            // Send response with appropriate status code
            var statusCode = result.StatusCode switch
            {
                SetPasswordResponseStatusCode.Success => 200,
                SetPasswordResponseStatusCode.UserNotFound => 404,
                SetPasswordResponseStatusCode.PasswordAlreadyExists => 400,
                SetPasswordResponseStatusCode.NewPasswordInvalid => 400,
                SetPasswordResponseStatusCode.PasswordMismatch => 400,
                SetPasswordResponseStatusCode.EmailInvalid => 400,
                SetPasswordResponseStatusCode.EmailAlreadyExists => 409,
                _ => 500
            };

            await SendAsync(response, statusCode, ct);
        }
    }
}
