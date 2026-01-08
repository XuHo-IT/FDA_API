using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace FDAAPI.Test.Drivers
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) { }
     
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check for a custom role header sent by the test
            Context.Request.Headers.TryGetValue("X-Test-Role", out var role);
            var activeRole = string.IsNullOrEmpty(role) ? "ADMIN" : role.ToString();

            // Additionally: Get the UserId from the Header; if none exists, generate a random one.
            Context.Request.Headers.TryGetValue("X-Test-UserId", out var userIdStr);
            var userId = string.IsNullOrEmpty(userIdStr) ? Guid.NewGuid().ToString() : userIdStr.ToString();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, "Test User"),
        new Claim(ClaimTypes.NameIdentifier, userId), 
        new Claim(ClaimTypes.Role, activeRole)
    };

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
