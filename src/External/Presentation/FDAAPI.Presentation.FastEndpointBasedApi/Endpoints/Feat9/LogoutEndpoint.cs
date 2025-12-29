using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG9;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat9.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat9
{
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
        private readonly IFeatureHandler<LogoutRequest, LogoutResponse> _handler;

        public LogoutEndpoint(IFeatureHandler<LogoutRequest, LogoutResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            // Define HTTP method and route
            Post("/api/v1/auth/logout");

            // Ideally require authentication, but allow anonymous for now
            // TODO: Uncomment when auth middleware is configured
            // Policies("User");
            AllowAnonymous();

            // API documentation
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
                // Step 1: Map DTO to application request
                var appRequest = new LogoutRequest
                {
                    RefreshToken = req.RefreshToken,
                    RevokeAllTokens = req.RevokeAllTokens
                };

                // Step 2: Execute handler
                var result = await _handler.ExecuteAsync(appRequest, ct);

                // Step 3: Map to response DTO
                var responseDto = new LogoutResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    TokensRevoked = result.TokensRevoked
                };

                // Step 4: Send response
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
