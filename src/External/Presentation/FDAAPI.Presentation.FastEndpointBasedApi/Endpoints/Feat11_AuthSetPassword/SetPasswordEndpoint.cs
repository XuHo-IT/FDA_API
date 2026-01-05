using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat11_AuthSetPassword.DTOs;
using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG11_AuthSetPassword;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat11_AuthSetPassword;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat11_AuthSetPassword{
    public class SetPasswordEndpoint : Endpoint<SetPasswordRequestDto, SetPasswordResponseDto>
    {
        private readonly IMediator _mediator;

        public SetPasswordEndpoint(IMediator mediator) => _mediator = mediator;

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

            var command = new SetPasswordRequest(userId, req.Email, req.NewPassword, req.ConfirmPassword);

            var result = await _mediator.Send(command, ct);

            var response = new SetPasswordResponseDto
            {
                Success = result.Success,
                Message = result.Message
            };

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









