using FastEndpoints;
using FastEndpoints.Swagger;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Infra.Configuration;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;

// Clear the default mapping so 'sub' remains 'sub' and 'role' remains 'role'
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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
    .AddCacheServices(configuration);


// FastEndpoints
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.DocumentSettings = s =>
        {
            s.Title = "FDA API";
            s.Version = "v1";
            s.Description = "Flood Detection & Alert API";

            // Define the Security Scheme
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
// BUILD APPLICATION
// ==================================================
var app = builder.Build();

// ==================================================
// MIDDLEWARE PIPELINE (ORDER MATTERS!)
// ==================================================

// 1. API Documentation (Development)
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options =>
    {
        options.Title = "FDA API Documentation";
        options.Theme = ScalarTheme.Purple;
    });
}

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 3. CORS (before authentication)
app.UseCors("CorsPolicy");

// 4. Authentication (NEW - MUST be before Authorization)
app.UseAuthentication();

// 5. Authorization (NEW - MUST be after Authentication)
app.UseAuthorization();

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
// DATABASE MIGRATION (Auto-apply on startup)
// ==================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Apply pending migrations
        context.Database.Migrate();


        Console.WriteLine("✅ Database migrated successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ An error occurred while migrating the database.");
    }
}

// ==================================================
// RUN APPLICATION
// ==================================================
app.Run();
