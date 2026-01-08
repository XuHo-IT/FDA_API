using DotNet.Testcontainers.Builders;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace FDAAPI.Test.Drivers
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        // Define PostgreSQL Container
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("fda_test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        // Define Redis Container for OAuth state and Caching
        private readonly RedisContainer _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // --- OVERRIDE POSTGRESQL CONFIGURATION ---
                var dbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (dbDescriptor != null) services.Remove(dbDescriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(_dbContainer.GetConnectionString()));

                // --- OVERRIDE REDIS CONFIGURATION ---
                var redisDescriptor = services.SingleOrDefault(
                    d => d.ServiceType.FullName?.Contains("StackExchangeRedis") == true);

                // We don't necessarily need to remove the old one if we use a different approach, 
                // but for testing, we re-register it with the container's connection string
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = _redisContainer.GetConnectionString();
                    options.InstanceName = "FDA_TEST_";
                });

                // Apply Migrations to the container database
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            });
        }

        // Starts containers before any tests run
        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            await _redisContainer.StartAsync();
        }

        // Stops and removes containers after all tests are finished
        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _redisContainer.StopAsync();
        }
    }
}