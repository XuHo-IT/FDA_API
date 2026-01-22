using FastEndpoints;
using FastEndpoints.Swagger;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Infra.Configuration;
using FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Feat54_MqttIngestion.Services;
using FDAAPI.Presentation.FastEndpointBasedApi.Hubs;
using FDAAPI.Presentation.FastEndpointBasedApi.Middleware;
using FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Analytics;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// ==================================================
// CONFIGURATION
// ==================================================
ConfigurationManager configuration = builder.Configuration;


//var envPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "..", ".env"));
//DotNetEnv.Env.Load(envPath);

// Add environment variables (for .env file support)
configuration.AddEnvironmentVariables();

// ==================================================
// SERVICE REGISTRATION
// ==================================================
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(configuration)
    .AddPersistenceServices(configuration)
    .AddAuthenticationServices(configuration)
    .AddCacheServices(configuration)
    .AddBackgroundJobs(configuration);

// ==================================================
// BACKGROUND JOBS REGISTRATION
// ==================================================
builder.Services.AddHostedService<FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Feat42_ProcessAlerts.AlertProcessingJob>();
builder.Services.AddHostedService<FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Feat43_DispatchNotifications.NotificationDispatchJob>();
builder.Services.AddHostedService<FDAAPI.Presentation.FastEndpointBasedApi.BackgroundJobs.Feat54_MqttIngestion.MqttIngestionJob>();
builder.Services.AddTransient<FrequencyAggregationRunner>();
builder.Services.AddTransient<SeverityAggregationRunner>();
builder.Services.AddTransient<HotspotAggregationRunner>();

// FastEndpoints
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "FDA API";
            s.Version = "v1";
            s.Description = "Flood Detection & Alert API - Authentication & Water Level Monitoring";
            s.AddSecurity("JWTBearerAuth", new NSwag.OpenApiSecurityScheme
            {
                Type = NSwag.OpenApiSecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                Description = "Enter your JWT Access Token"
            });

            // Global Security Requirement using PostProcess
            s.PostProcess = doc =>
            {
                doc.Security.Add(new NSwag.OpenApiSecurityRequirement
                {
                    { "JWTBearerAuth", Array.Empty<string>() }
                });
            };
        };
    });

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {

        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// ==================================================
// SIGNALR CONFIGURATION
// ==================================================
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // For development
    options.MaximumReceiveMessageSize = 102400; // 100KB
});

// Register Realtime Service
builder.Services.AddScoped<IRealtimeMapService, RealtimeMapService>();

// ==================================================
// BUILD APPLICATION
// ==================================================
var app = builder.Build();

// ==================================================
// MIDDLEWARE PIPELINE (ORDER MATTERS!)
// ==================================================

// 0. Global Exception Handler (MUST be first to catch all exceptions)
app.UseMiddleware<ValidationExceptionMiddleware>();

// 1. API Documentation (Development)
// 1. API Documentation (Bật Swagger cho cả Dev và UAT)
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("UAT"))
{
    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/swagger";
        settings.DocumentPath = "/swagger/v1/swagger.json";
        settings.DocumentTitle = "FDA API v1";
    });
}


// 2. HTTPS Redirection
// QUAN TRỌNG: Chỉ bật ở Local. Trên VPS Nginx sẽ lo, bật ở đây sẽ bị lỗi vòng lặp Redirect.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 3. CORS (before authentication)
app.UseCors("CorsPolicy");

// 4. Authentication (NEW - MUST be before Authorization)
app.UseAuthentication();

// 5. Authorization (NEW - MUST be after Authentication)
app.UseAuthorization();

// 5.5. Hangfire Dashboard (for monitoring background jobs)
app.UseHangfireDashboard("/hangfire", new Hangfire.DashboardOptions
{
    Authorization = new[] { new FDAAPI.Presentation.FastEndpointBasedApi.HangfireAuthorizationFilter() },
    DashboardTitle = "FDA API Background Jobs"
});

// 5.6. Register recurring analytics jobs (after app startup)
app.RegisterAnalyticsRecurringJobs();

// 6. FastEndpoints (MUST be after Auth middleware)
app.UseFastEndpoints(config =>
{
    //config.Endpoints.RoutePrefix = "api";

    // Global error handler
    config.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
    {
        return new
        {
            success = false,
            message = "Validation failed",
            errors = failures.Select(f => new
            {
                field = f.PropertyName,
                message = f.ErrorMessage
            })
        };
    };
});

// 7. Swagger (after FastEndpoints)
app.UseSwaggerGen();

// ==================================================
// SIGNALR HUB MAPPING
// ==================================================
app.MapHub<FloodDataHub>("/hubs/flood-data");

// ==================================================
// DATABASE MIGRATION (Auto-apply on startup)
// ==================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (app.Environment.IsEnvironment("UAT"))
        {
            context.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS uat_schema;");
        }
        // Apply pending migrations
        context.Database.Migrate();


        Console.WriteLine("? Database migrated successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "? An error occurred while migrating the database.");
    }
}

// ==================================================
// RUN APPLICATION
// ==================================================
app.Run();

public partial class Program { }






