using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG8;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat8.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat8
{
    /// <summary>
    /// Endpoint for refreshing access token
    /// Implements token rotation for security
    /// 
    /// Request Flow:
    ///   1. Client sends POST with current refresh token
    ///   2. Handler validates refresh token
    ///   3. Generates new access token + new refresh token
    ///   4. Revokes old refresh token (prevent reuse)
    ///   5. Returns new tokens
    /// </summary>
    public class RefreshTokenEndpoint : Endpoint<RefreshTokenRequestDto, RefreshTokenResponseDto>
    {
        private readonly IMediator _mediator;

        public RefreshTokenEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/auth/refresh-token");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Refresh access token";
                s.Description = "Get a new access token using a valid refresh token. " +
                               "Old refresh token is automatically revoked (token rotation).";
                s.ExampleRequest = new RefreshTokenRequestDto
                {
                    RefreshToken = "base64_encoded_refresh_token"
                };
                s.ResponseExamples[200] = new RefreshTokenResponseDto
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = "new_access_token",
                    RefreshToken = "new_refresh_token",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                };
            });
            Tags("Authentication", "Token");
        }

        public override async Task HandleAsync(RefreshTokenRequestDto req, CancellationToken ct)
        {
            try
            {
                var command = new RefreshTokenRequest(req.RefreshToken);

                var result = await _mediator.Send(command, ct);

                var responseDto = new RefreshTokenResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.ExpiresAt
                };

                if (result.Success)
                {
                    await SendAsync(responseDto, 200, ct);
                }
                else
                {
                    await SendAsync(responseDto, 401, ct);
                }
            }
            catch (Exception ex)
            {
                var errorDto = new RefreshTokenResponseDto
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}
