using FastEndpoints;
using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG15;
using FDAAPI.App.FeatG19;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat15.DTOs;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat18.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat18
{
    public class VerifyUpdatePhoneEndpoint : Endpoint<VerifyUpdatePhoneRequestDto, UpdateProfileResponseDto>
    {
        private readonly IMediator _mediator;

        public VerifyUpdatePhoneEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/user-profile/update-phoneNumber");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
            Summary(s =>
            {
                s.Summary = "Verify and update user's phone number";

                s.Description =
                    "Verifies the OTP code sent to the new phone number and updates the user's profile upon successful verification. " +
                    "This endpoint requires authentication. If verification succeeds, the phone number is updated immediately.";

                s.ExampleRequest = new VerifyUpdatePhoneRequestDto
                {
                    NewPhoneNumber = "+84901234567",
                    OtpCode = "123456"
                };

                s.ResponseExamples[200] = new UpdateProfileResponseDto
                {
                    Success = true,
                    Message = "Phone number updated successfully",
                    Profile = new UserProfileDto
                    {
                        PhoneNumber = "+84901234567",
                        FullName = "",        
                        Email = "",           
                        AvatarUrl = ""        
                    }
                };

            });

            Tags("User Profile", "Verification");
        }

        public override async Task HandleAsync(VerifyUpdatePhoneRequestDto req, CancellationToken ct)
        {
            var userIdClaim =
                User.FindFirst("sub")?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                await SendAsync(new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var command = new VerifyAndUpdatePhoneRequest(userId, req.NewPhoneNumber, req.OtpCode);

            var result = await _mediator.Send(command, ct);

            await SendAsync(new UpdateProfileResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Profile = result.Profile
            }, result.Success ? 200 : 400, ct);

        }

    }
}
