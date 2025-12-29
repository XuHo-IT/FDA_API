using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG10;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat10.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat10
{
    public class ChangePasswordEndpoint : Endpoint<ChangePasswordRequestDto, ChangePasswordResponseDto>
    {
        private readonly IFeatureHandler<ChangePasswordRequest, ChangePasswordResponse> _handler;

        public ChangePasswordEndpoint(IFeatureHandler<ChangePasswordRequest, ChangePasswordResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            Post("/api/v1/auth/change-password");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Summary(s =>
            {
                s.Summary = "Change password for authenticated user";
                s.Description = "Allows users to change their existing password. Requires current password verification. All refresh tokens will be invalidated.";
                s.ExampleRequest = new ChangePasswordRequestDto
                {
                    CurrentPassword = "OldPassword123!",
                    NewPassword = "NewPassword456!",
                    ConfirmPassword = "NewPassword456!"
                };
            });
            Tags("Authentication", "Password Management");
        }

        public override async Task HandleAsync(ChangePasswordRequestDto req, CancellationToken ct)
        {
            // Extract user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = "Invalid user authentication"
                }, 401, ct);
                return;
            }

            // Map DTO to Request
            var appRequest = new ChangePasswordRequest
            {
                UserId = userId,
                CurrentPassword = req.CurrentPassword,
                NewPassword = req.NewPassword,
                ConfirmPassword = req.ConfirmPassword
            };

            // Execute handler
            var result = await _handler.ExecuteAsync(appRequest, ct);

            // Map Response to DTO
            var response = new ChangePasswordResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            // Send response with appropriate status code
            var statusCode = result.StatusCode switch
            {
                ChangePasswordResponseStatusCode.Success => 200,
                ChangePasswordResponseStatusCode.UserNotFound => 404,
                ChangePasswordResponseStatusCode.CurrentPasswordIncorrect => 401,
                ChangePasswordResponseStatusCode.NewPasswordInvalid => 400,
                ChangePasswordResponseStatusCode.PasswordMismatch => 400,
                ChangePasswordResponseStatusCode.SameAsCurrentPassword => 400,
                _ => 500
            };

            await SendAsync(response, statusCode, ct);
        }
    }
}
