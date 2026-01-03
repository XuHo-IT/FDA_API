using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG15;
using FDAAPI.App.FeatG19;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat15.DTOs;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat18.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat18
{
    public class VerifyUpdatePhoneEndpoint : Endpoint<VerifyUpdatePhoneRequestDto, UpdateProfileResponseDto>
    {
        private readonly IFeatureHandler<VerifyAndUpdatePhoneRequest, UpdateProfileResponse> _handler;

        public VerifyUpdatePhoneEndpoint(IFeatureHandler<VerifyAndUpdatePhoneRequest, UpdateProfileResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            Post("/api/v1/user-profile/update-phoneNumber");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(VerifyUpdatePhoneRequestDto req, CancellationToken ct)
        {
            // check authenticated user
            var userIdClaim = User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                await SendAsync(new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var result = await _handler.ExecuteAsync(new VerifyAndUpdatePhoneRequest
            {
                UserId = Guid.Parse(userIdClaim),
                NewPhoneNumber = req.NewPhoneNumber,
                OtpCode = req.OtpCode
            }, ct);

            await SendAsync(new UpdateProfileResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Profile = result.Profile
            }, result.Success ? 200 : 400, ct);
        }
    }
}
