using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG9_AuthLogout;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat9_AuthLogout.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat9_AuthLogout{
    /// <summary>
    /// Endpoint for user logout
    /// Supports single device or all devices logout
    /// 
    /// Request Flow:
    ///   1. Client sends POST with refresh token
    ///   2. Handler revokes token(s)
    ///   3. Returns success/failure
    /// </summary>
    public class LogoutEndpoint : Endpoint<LogoutRequestDto, LogoutResponseDto>
    {
        private readonly IMediator _mediator;

        public LogoutEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/auth/logout");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "User logout";
                s.Description = "Invalidate refresh token and logout user. " +
                               "Optionally logout from all devices by setting revokeAllTokens=true.";
                s.ExampleRequest = new LogoutRequestDto
                {
                    RefreshToken = "current_refresh_token",
                    RevokeAllTokens = false
                };
                s.ResponseExamples[200] = new LogoutResponseDto
                {
                    Success = true,
                    Message = "Logout successful",
                    TokensRevoked = 1
                };
            });
            Tags("Authentication", "Logout");
        }

        public override async Task HandleAsync(LogoutRequestDto req, CancellationToken ct)
        {
            try
            {
                var command = new LogoutRequest(req.RefreshToken, req.RevokeAllTokens);

                var result = await _mediator.Send(command, ct);

                var responseDto = new LogoutResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    TokensRevoked = result.TokensRevoked
                };

                await SendAsync(responseDto, 200, ct);
            }
            catch (Exception ex)
            {
                var errorDto = new LogoutResponseDto
                {
                    Success = false,
                    Message = $"An unexpected error occurred: {ex.Message}",
                    TokensRevoked = 0
                };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}









