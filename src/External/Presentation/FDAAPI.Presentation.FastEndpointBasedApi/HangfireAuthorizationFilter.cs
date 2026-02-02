using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;

namespace FDAAPI.Presentation.FastEndpointBasedApi
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var http = context.GetHttpContext();
            var env = http.RequestServices.GetRequiredService<IWebHostEnvironment>();

            // Allow in Development
            if (env.IsDevelopment() || env.IsEnvironment("UAT")) 
                return true;

            // Production: must be authenticated
            return http.User.Identity?.IsAuthenticated == true;
        }
    }
}
