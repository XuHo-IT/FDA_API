using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG16;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat16.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat14
{
    /// <summary>
    /// Endpoint for Google OAuth login from mobile apps (React Native)
    /// POST /api/v1/auth/google/mobile
    /// </summary>
    public class GoogleMobileLoginEndpoint : Endpoint<GoogleMobileLoginRequestDto, GoogleMobileLoginResponseDto>
    {
        private readonly IFeatureHandler<GoogleMobileLoginRequest, GoogleMobileLoginResponse> _handler;

        public GoogleMobileLoginEndpoint(IFeatureHandler<GoogleMobileLoginRequest, GoogleMobileLoginResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            Post("/api/v1/auth/google/mobile");
            AllowAnonymous();
            Tags("Authentication", "Google OAuth", "Mobile");
            Summary(s =>
            {
                s.Summary = "Google OAuth login for mobile apps";
                s.Description = "Accepts idToken from React Native Google Sign-In SDK and returns FDA API JWT tokens";
                s.ExampleRequest = new GoogleMobileLoginRequestDto
                {
                    IdToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjE4MmU0NTBhMzVhMjA4MWZhYTFkOWFlMjkwZjE1ZGM1NjJiMGY1ZDMiLCJ0eXAiOiJKV1QifQ..."
                };
            });
        }

        public override async Task HandleAsync(GoogleMobileLoginRequestDto req, CancellationToken ct)
        {
            var appRequest = new GoogleMobileLoginRequest
            {
                IdToken = req.IdToken
            };

            var result = await _handler.ExecuteAsync(appRequest, ct);

            var response = new GoogleMobileLoginResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt,
                User = result.User != null ? new DTOs.UserDto
                {
                    Id = result.User.Id,
                    Email = result.User.Email,
                    FullName = result.User.FullName,
                    AvatarUrl = result.User.AvatarUrl,
                    Roles = result.User.Roles
                } : null
            };

            await SendAsync(response, cancellation: ct);
        }
    }
}
