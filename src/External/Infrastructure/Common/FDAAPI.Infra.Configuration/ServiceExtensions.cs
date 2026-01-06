using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.App.FeatG1_WaterLevelCreate;
using FDAAPI.App.FeatG2_WaterLevelUpdate;
using FDAAPI.App.FeatG3_WaterLevelGet;
using FDAAPI.App.FeatG4_WaterLevelDelete;
using FDAAPI.App.FeatG6_AuthSendOtp;
using FDAAPI.App.FeatG7_AuthLogin;
using FDAAPI.App.FeatG8_AuthRefreshToken;
using FDAAPI.App.FeatG9_AuthLogout;
using FDAAPI.App.FeatG10_AuthChangePassword;
using FDAAPI.App.FeatG11_AuthSetPassword;
using FDAAPI.App.FeatG12_AuthGoogleLoginInitiate;
using FDAAPI.App.FeatG13_AuthGoogleOAuthCallback;
using FDAAPI.App.FeatG14_ProfileGet;
using FDAAPI.App.FeatG15_ProfileUpdate;
using FDAAPI.App.FeatG16_AuthGoogleMobileLogin;
using FDAAPI.App.FeatG17_AuthCheckIdentifier;
using FDAAPI.App.FeatG19_ProfileVerifyUpdatePhone;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Infra.Persistence.Repositories;
using FDAAPI.Infra.Services.Auth;
using FDAAPI.Infra.Services.Cache;
using FDAAPI.Infra.Services.OAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using FDAAPI.App.FeatG23_StationCreate;
using FDAAPI.App.FeatG25_StationUpdate;
using FDAAPI.App.FeatG24_StationGet;
using FDAAPI.App.FeatG26_StationDelete;
using FDAAPI.App.FeatG21_UserList;
using FDAAPI.App.FeatG25_StationList;
using FDAAPI.App.FeatG22_UserUpdate;

namespace FDAAPI.Infra.Configuration
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfraConfiguration(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // 1. Đọc Connection String từ hai nguồn (Ưu tiên ConnectionStrings__PostgreSQLConnection)
            var connectionString = configuration.GetConnectionString("PostgreSQLConnection");

            // 2. Nếu GetConnectionString bị lỗi (trả về null), đọc từ biến môi trường cấp cao hơn (POSTGRES_CONN_STRING)
            if (string.IsNullOrEmpty(connectionString))
            {
                // Đọc trực tiếp từ cấu hình, nơi biến môi trường được inject
                connectionString = configuration["POSTGRES_CONN_STRING"];

                // *** KHÔNG CẦN THIẾT, NHƯNG LÀ PHƯƠNG ÁN DỰ PHÒNG CUỐI CÙNG:
                // connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING"); 
            }

            // 3. Kiểm tra xem chuỗi kết nối đã tồn tại chưa
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("PostgreSQL Connection String is not initialized or found in environment variables.");
            }

            // 4. Cấu hình DbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // Nếu đây là Development hoặc Staging, hãy tự động chạy Migration
            // Dù trong Production, Migration nên được chạy thủ công trước
            if (env.IsDevelopment() || env.IsStaging())
            {
                // Thêm đoạn code này để đảm bảo Migration được áp dụng tự động 
                // sau khi DBContext đã được đăng ký.
                // Lưu ý: Việc này yêu cầu AppDbContext phải được resolved.
                // Tuy nhiên, chúng ta sẽ để việc thực thi Migrate() ở Program.cs để đảm bảo DbContext đã được Service Provider tạo ra.
            }

            return services;
        }
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Existing JWT and Password services
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            // Add HttpClient for Google OAuth API calls
            services.AddHttpClient();

            // Google OAuth Service
            services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
            services.AddScoped<IUserProfileMapper, UserProfileMapper>();
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(VerifyAndUpdatePhoneRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(SendOtpRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(LoginRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(LogoutRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(RefreshTokenRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(ChangePasswordRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(GoogleLoginInitiateRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(GoogleOAuthCallbackRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(GoogleMobileLoginRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(GetProfileRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(SetPasswordRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(CheckIdentifierRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(GetUsersRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(UpdateProfileRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(CreateStationRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(UpdateStationRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(GetStationRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(GetStationsRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(DeleteStationRequest).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(UpdateUserRequest).Assembly);

            });
            services.AddTransient<IFeatureHandler<CreateWaterLevelRequest, CreateWaterLevelResponse>, CreateWaterLevelHandler>();
            services.AddTransient<IFeatureHandler<UpdateWaterLevelRequest, UpdateWaterLevelResponse>, UpdateWaterLevelHandler>();
            services.AddTransient<IFeatureHandler<GetWaterLevelRequest, GetWaterLevelResponse>, GetWaterLevelHandler>();
            services.AddTransient<IFeatureHandler<DeleteWaterLevelRequest, DeleteWaterLevelResponse>, DeleteWaterLevelHandler>();

            services.AddHttpClient<IImageStorageService, ImageKitService>();
            services.AddScoped<IImageUploadPolicy, ImageUploadPolicy>();
            return services;
        }
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Try multiple ways to get connection string (same as AddInfraConfiguration)
            var connectionString = configuration.GetConnectionString("PostgreSQLConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IWaterLevelRepository, PgsqlWaterLevelRepository>();
            services.AddScoped<IStationRepository, PgslStationRepository>();
            services.AddScoped<IUserRepository, PgsqlUserRepository>();
            services.AddScoped<IRoleRepository, PgsqlRoleRepository>();
            services.AddScoped<IUserRoleRepository, PgsqlUserRoleRepository>();
            services.AddScoped<IRefreshTokenRepository, PgsqlRefreshTokenRepository>();
            services.AddScoped<IOtpCodeRepository, PgsqlOtpCodeRepository>();

            // OAuth provider repository
            services.AddScoped<IUserOAuthProviderRepository, PgsqlUserOAuthProviderRepository>();

            return services;
        }

        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // JWT Authentication
            var jwtSecret = configuration["Jwt:Secret"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret is not configured in appsettings.json");
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                        ClockSkew = TimeSpan.Zero, // Strict expiration validation
                        NameClaimType = "sub",
                        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",


                    };

                    // Optional: Event handlers for debugging
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers["Token-Expired"] = "true";
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // Authorization Policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SUPERADMIN"));
                options.AddPolicy("Admin", policy => policy.RequireRole("ADMIN", "SUPERADMIN"));
                options.AddPolicy("Authority", policy => policy.RequireRole("AUTHORITY", "ADMIN", "SUPERADMIN"));
                options.AddPolicy("User", policy => policy.RequireRole("USER", "AUTHORITY", "ADMIN", "SUPERADMIN"));
            });

            return services;
        }

        public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Redis distributed cache
            var redisConnection = configuration.GetConnectionString("RedisConnection");

            if (string.IsNullOrEmpty(redisConnection))
            {
                throw new InvalidOperationException("Redis Connection String is not configured");
            }

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "FDA_API_";
                options.ConfigurationOptions = StackExchange.Redis.ConfigurationOptions.Parse(redisConnection);
                options.ConfigurationOptions.AbortOnConnectFail = false;
                options.ConfigurationOptions.ConnectTimeout = 15000; // 15 seconds
                options.ConfigurationOptions.SyncTimeout = 10000; // 10 seconds
                options.ConfigurationOptions.AsyncTimeout = 10000; // 10 seconds
            });

            // Register state cache service for OAuth
            services.AddScoped<IStateCache, RedisStateCache>();

            return services;
        }

    }
}

