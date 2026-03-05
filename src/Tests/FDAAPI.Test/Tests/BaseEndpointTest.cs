using FDAAPI.Test.Drivers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FDAAPI.Test.Tests
{
    public abstract class BaseEndpointTest : IClassFixture<ApiWebApplicationFactory>
    {
        protected readonly ApiWebApplicationFactory _factory;
        protected readonly HttpClient _client;

        protected BaseEndpointTest(ApiWebApplicationFactory factory)
        {
            _factory = factory;

            // Create a client that uses the Fake Auth Handler
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Register the TestAuthHandler under a specific scheme
                    services.AddAuthentication("TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                });
            }).CreateClient();

            // Set the default authorization header for all requests in this test class
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("TestScheme");
        }
    }
}

