using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Web;

namespace FDAAPI.Infra.Services.OAuth
{
    public class GoogleOAuthService : IGoogleOAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private string ClientId => _configuration["OAuth:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
        private string ClientSecret => _configuration["OAuth:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
        private string RedirectUri => _configuration["OAuth:Google:RedirectUri"] ?? "https://localhost:7097/api/v1/auth/google/callback";

        // Get all allowed client IDs (Web + Mobile)
        private List<string> AllowedClientIds
        {
            get
            {
                var clientIds = new List<string> { ClientId };

                // Add mobile client IDs if configured
                var mobileClientIds = _configuration.GetSection("OAuth:Google:MobileClientIds").Get<string[]>();
                if (mobileClientIds != null && mobileClientIds.Length > 0)
                {
                    clientIds.AddRange(mobileClientIds);
                }

                return clientIds;
            }
        }

        public GoogleOAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public string GenerateAuthorizationUrl(string state)
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["client_id"] = ClientId;
            queryParams["redirect_uri"] = RedirectUri;
            queryParams["response_type"] = "code";
            queryParams["scope"] = "openid email profile";
            queryParams["state"] = state;
            queryParams["access_type"] = "online";
            queryParams["prompt"] = "select_account";

            return $"https://accounts.google.com/o/oauth2/v2/auth?{queryParams}";
        }

        public async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "redirect_uri", RedirectUri },
                { "grant_type", "authorization_code" }
            });

            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", requestBody, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Failed to exchange authorization code: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(content);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                throw new InvalidOperationException("Invalid token response from Google");
            }

            return tokenResponse;
        }

        public async Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken, CancellationToken ct = default)
        {
            var httpClient = _httpClientFactory.CreateClient();

            // Use Google's tokeninfo endpoint to verify and get user info
            var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}", ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Failed to verify ID token: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfoResponse>(content);

            if (tokenInfo == null)
            {
                throw new InvalidOperationException("Invalid token info response from Google");
            }

            // Verify audience matches one of our allowed client IDs (Web + Mobile)
            var allowedIds = AllowedClientIds;
            if (!allowedIds.Contains(tokenInfo.Audience))
            {
                throw new InvalidOperationException(
                    $"Token audience '{tokenInfo.Audience}' does not match any allowed client IDs. " +
                    $"Allowed: {string.Join(", ", allowedIds)}"
                );
            }

            // Map to GoogleUserInfo
            return new GoogleUserInfo
            {
                Id = tokenInfo.Subject,
                Email = tokenInfo.Email,
                Name = tokenInfo.Name,
                Picture = tokenInfo.Picture,
                EmailVerified = tokenInfo.EmailVerified.ToLower() == "true"
            };
        }
    }
}






