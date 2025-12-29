using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Services.OAuth
{
    public interface IGoogleOAuthService
    {
        string GenerateAuthorizationUrl(string state);
        Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default);
        Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken, CancellationToken ct = default);
    }
}
