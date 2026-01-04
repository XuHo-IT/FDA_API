using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG12;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat12.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat12
{
    /// <summary>
    /// Endpoint to initiate Google OAuth login flow
    /// Returns authorization URL for user to consent
    /// </summary>
    public class GoogleLoginInitiateEndpoint : Endpoint<GoogleLoginInitiateRequestDto, GoogleLoginInitiateResponseDto>
    {
        private readonly IMediator _mediator;

        public GoogleLoginInitiateEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Get("/api/v1/auth/google");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Initiate Google OAuth login";
                s.Description = "Generates Google authorization URL with CSRF state token. " +
                               "Client should redirect user to this URL for Google consent.";

                s.ExampleRequest = new GoogleLoginInitiateRequestDto
                {
                    ReturnUrl = "https://fda.gov.vn/dashboard"
                };

                s.ResponseExamples[200] = new GoogleLoginInitiateResponseDto
                {
                    Success = true,
                    Message = "Redirect to Google for authentication",
                    AuthorizationUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=...&state=...",
                    State = "base64_state_token"
                };
            });
            Tags("Authentication", "Google OAuth");
        }

        public override async Task HandleAsync(GoogleLoginInitiateRequestDto req, CancellationToken ct)
        {
            try
            {
                var request = new GoogleLoginInitiateRequest(req.ReturnUrl);

                var result = await _mediator.Send(request, ct);

                var responseDto = new GoogleLoginInitiateResponseDto
                {
                    Success = result.Success,
                    Message = result.Message,
                    AuthorizationUrl = result.AuthorizationUrl,
                    State = result.State
                };

                if (result.Success)
                {
                    await SendAsync(responseDto, 200, ct);
                }
                else
                {
                    await SendAsync(responseDto, 400, ct);
                }
            }
            catch (Exception ex)
            {
                var errorDto = new GoogleLoginInitiateResponseDto
                {
                    Success = false,
                    Message = $"Failed to initiate Google login: {ex.Message}"
                };
                await SendAsync(errorDto, 500, ct);
            }
        }
    }
}
