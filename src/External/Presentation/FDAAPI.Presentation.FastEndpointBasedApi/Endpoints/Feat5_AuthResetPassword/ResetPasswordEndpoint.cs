using FastEndpoints;
using FDAAPI.App.Common.Models.Auth;
using FDAAPI.App.FeatG5_AuthResetPassword;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat5_AuthResetPassword.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat5_AuthResetPassword
{
    public class ResetPasswordEndpoint : Endpoint<ResetPasswordRequestDto, ResetPasswordResponseDto>
    {
        private readonly IMediator _mediator;

        public ResetPasswordEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/auth/reset-password");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Summary(s =>
            {
                s.Summary = "Reset password for authenticated user (forgot password flow)";
                s.Description = "Allows users who logged in via OTP to reset their password without providing the old password. All refresh tokens will be invalidated.";
                s.ExampleRequest = new ResetPasswordRequestDto
                {
                    NewPassword = "NewPassword123!",
                    ConfirmPassword = "NewPassword123!"
                };
            });
            Tags("Authentication", "Password Management");
        }

        public override async Task HandleAsync(ResetPasswordRequestDto req, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new ResetPasswordResponseDto
                {
                    Success = false,
                    Message = "Invalid user authentication"
                }, 401, ct);
                return;
            }

            var command = new ResetPasswordRequest(userId, req.NewPassword, req.ConfirmPassword);
            var result = await _mediator.Send(command, ct);

            var response = new ResetPasswordResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

            var statusCode = result.StatusCode switch
            {
                ResetPasswordResponseStatusCode.Success => 200,
                ResetPasswordResponseStatusCode.UserNotFound => 404,
                ResetPasswordResponseStatusCode.NewPasswordInvalid => 400,
                ResetPasswordResponseStatusCode.PasswordMismatch => 400,
                ResetPasswordResponseStatusCode.SameAsCurrentPassword => 400,
                _ => 500
            };

            await SendAsync(response, statusCode, ct);
        }
    }
}
