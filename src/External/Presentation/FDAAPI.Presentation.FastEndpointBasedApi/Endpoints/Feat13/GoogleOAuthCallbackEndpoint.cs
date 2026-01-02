using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG13;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat13.DTOs;
using Microsoft.Extensions.Configuration;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat13
{
    /// <summary>
    /// Endpoint to handle Google OAuth callback
    /// Processes authorization code and issues FDA API tokens
    /// </summary>
    public class GoogleOAuthCallbackEndpoint : Endpoint<GoogleOAuthCallbackRequestDto, GoogleOAuthCallbackResponseDto>
    {
        private readonly IFeatureHandler<GoogleOAuthCallbackRequest, GoogleOAuthCallbackResponse> _handler;
        private readonly IConfiguration _configuration;

        public GoogleOAuthCallbackEndpoint(
            IFeatureHandler<GoogleOAuthCallbackRequest, GoogleOAuthCallbackResponse> handler,
            IConfiguration configuration)
        {
            _handler = handler;
            _configuration = configuration;
        }

        public override void Configure()
        {
            Get("/api/v1/auth/google/callback");
            AllowAnonymous();

            Summary(s =>
            {
                s.Summary = "Google OAuth callback handler";
                s.Description = "Receives authorization code from Google, validates state token, " +
                               "creates or links user account, and returns FDA API access tokens. " +
                               "New users are automatically assigned USER (Citizen) role.";

                s.ExampleRequest = new GoogleOAuthCallbackRequestDto
                {
                    Code = "4/0AY0e-g7...",
                    State = "base64_state_token"
                };

                s.ResponseExamples[200] = new GoogleOAuthCallbackResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                    RefreshToken = "base64_encoded_refresh_token",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = new UserDto
                    {
                        Id = Guid.NewGuid(),
                        Email = "user@gmail.com",
                        FullName = "User Name",
                        AvatarUrl = "https://lh3.googleusercontent.com/...",
                        Roles = new List<string> { "USER" }
                    }
                };

                s.ResponseExamples[400] = new GoogleOAuthCallbackResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired state token"
                };
            });

            Tags("Authentication", "Google OAuth");
        }

        public override async Task HandleAsync(GoogleOAuthCallbackRequestDto req, CancellationToken ct)
        {
            try
            {
                // Map query params to application request
                var appRequest = new GoogleOAuthCallbackRequest
                {
                    Code = req.Code,
                    State = req.State
                };

                var result = await _handler.ExecuteAsync(appRequest, ct);

                // Map to response DTO
                var responseDto = new GoogleOAuthCallbackResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.ExpiresAt,
                    ReturnUrl = result.ReturnUrl,
                    User = result.User != null ? new UserDto
                    {
                        Id = result.User.Id,
                        Email = result.User.Email,
                        FullName = result.User.FullName,
                        AvatarUrl = result.User.AvatarUrl,
                        Roles = result.User.Roles
                    } : null
                };

                // if (result.Success)
                // {
                //     await SendAsync(responseDto, 200, ct);
                // }
                // else
                // {
                //     // 400 Bad Request for invalid state/code
                //     await SendAsync(responseDto, 400, ct);
                // }

                if (result.Success)
                {
                    // Get frontend URL from configuration
                    var frontendBaseUrl = _configuration["OAuth:FrontendUrl"] ?? "http://localhost:3000";
                    var returnPath = result.ReturnUrl ?? "/dashboard";

                    // Build redirect URL with tokens in fragment (#)
                    // Fragment (#) ensures tokens are not sent to server in subsequent requests
                    var redirectUrl = $"{frontendBaseUrl}/auth/callback#access_token={Uri.EscapeDataString(result.AccessToken)}&refresh_token={Uri.EscapeDataString(result.RefreshToken)}&return_url={Uri.EscapeDataString(returnPath)}";

                    // Use HttpContext.Response.Redirect for external URLs
                    HttpContext.Response.Redirect(redirectUrl, permanent: false);
                }
                else
                {
                    // Redirect to login page with error message
                    var frontendBaseUrl = _configuration["OAuth:FrontendUrl"] ?? "http://localhost:3000";
                    var errorUrl = $"{frontendBaseUrl}/login?error={Uri.EscapeDataString(result.Message)}";

                    HttpContext.Response.Redirect(errorUrl, permanent: false);
                }
            }
            catch (Exception ex)
            {
                var errorDto = new GoogleOAuthCallbackResponseDto
                {
                    Success = false,
                    Message = $"OAuth callback error: {ex.Message}"
                };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}
