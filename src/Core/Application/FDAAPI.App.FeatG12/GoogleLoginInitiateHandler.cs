using FDAAPI.App.Common.Features;
using FDAAPI.Infra.Services.Cache;
using FDAAPI.Infra.Services.OAuth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG12
{
    public class GoogleLoginInitiateHandler : IRequestHandler<GoogleLoginInitiateRequest, GoogleLoginInitiateResponse>
    {
        private readonly IGoogleOAuthService _googleOAuthService;
        private readonly IStateCache _stateCache;

        public GoogleLoginInitiateHandler(
            IGoogleOAuthService googleOAuthService,
            IStateCache stateCache)
        {
            _googleOAuthService = googleOAuthService;
            _stateCache = stateCache;
        }

        public async Task<GoogleLoginInitiateResponse> Handle(
            GoogleLoginInitiateRequest request,
            CancellationToken ct = default)
        {
            try
            {
                // Generate CSRF state token (Base64-encoded GUID)
                var state = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_'); // URL-safe Base64

                // Store state in Redis with 5-minute TTL
                await _stateCache.SetStateAsync(state, request.ReturnUrl, TimeSpan.FromMinutes(5), ct);

                // Generate Google OAuth authorization URL
                var authorizationUrl = _googleOAuthService.GenerateAuthorizationUrl(state);

                return new GoogleLoginInitiateResponse
                {
                    Success = true,
                    Message = "Redirect to Google for authentication",
                    AuthorizationUrl = authorizationUrl,
                    State = state
                };
            }
            catch (Exception ex)
            {
                return new GoogleLoginInitiateResponse
                {
                    Success = false,
                    Message = $"Failed to initiate Google login: {ex.Message}"
                };
            }
        }
    }
}
