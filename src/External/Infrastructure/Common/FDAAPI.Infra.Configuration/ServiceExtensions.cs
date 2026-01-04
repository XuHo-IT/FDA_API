using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.App.Feat1;
using FDAAPI.App.Feat2;
using FDAAPI.App.Feat3;
using FDAAPI.App.Feat4;
using FDAAPI.App.Feat5;
using FDAAPI.App.FeatG10;
using FDAAPI.App.FeatG11;
using FDAAPI.App.FeatG12;
using FDAAPI.App.FeatG13;
using FDAAPI.App.FeatG14;
using FDAAPI.App.FeatG15;
using FDAAPI.App.FeatG16;
using FDAAPI.App.FeatG17;
using FDAAPI.App.FeatG19;
using FDAAPI.App.FeatG6;
using FDAAPI.App.FeatG7;
using FDAAPI.App.FeatG8;
using FDAAPI.App.FeatG9;
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
            });
            services.AddTransient<IFeatureHandler<CreateWaterLevelRequest, CreateWaterLevelResponse>, CreateWaterLevelHandler>();
            services.AddTransient<IFeatureHandler<UpdateWaterLevelRequest, UpdateWaterLevelResponse>, UpdateWaterLevelHandler>();
            services.AddTransient<IFeatureHandler<GetWaterLevelRequest, GetWaterLevelResponse>, GetWaterLevelHandler>();
            services.AddTransient<IFeatureHandler<DeleteWaterLevelRequest, DeleteWaterLevelResponse>, DeleteWaterLevelHandler>();
            services.AddTransient<IFeatureHandler<GetStaticDataRequest, GetStaticDataResponse>, GetStaticDataHandler>();

            //services.AddTransient<IFeatureHandler<SendOtpRequest, SendOtpResponse>, SendOtpHandler>();
            //services.AddTransient<IFeatureHandler<LoginRequest, LoginResponse>, LoginHandler>();
            //services.AddTransient<IFeatureHandler<LogoutRequest, LogoutResponse>, LogoutHandler>();
            //services.AddTransient<IFeatureHandler<RefreshTokenRequest, RefreshTokenResponse>, RefreshTokenHandler>();
            //services.AddTransient<IFeatureHandler<ChangePasswordRequest, ChangePasswordResponse>, ChangePasswordHandler>();
            //services.AddTransient<IFeatureHandler<SetPasswordRequest, SetPasswordResponse>, SetPasswordHandler>();
            //services.AddTransient<IFeatureHandler<GoogleLoginInitiateRequest, GoogleLoginInitiateResponse>, GoogleLoginInitiateHandler>();
            //services.AddTransient<IFeatureHandler<GoogleOAuthCallbackRequest, GoogleOAuthCallbackResponse>, GoogleOAuthCallbackHandler>();
            //services.AddTransient<IFeatureHandler<GoogleMobileLoginRequest, GoogleMobileLoginResponse>, GoogleMobileLoginHandler>();
            //services.AddTransient<IFeatureHandler<GetProfileRequest, GetProfileResponse>, GetProfileHandler>();
            //services.AddTransient<IFeatureHandler<UpdateProfileRequest, UpdateProfileResponse>, UpdateProfileHandler>();
            //services.AddTransient<IFeatureHandler<VerifyAndUpdatePhoneRequest, UpdateProfileResponse>, VerifyAndUpdatePhoneHandler>();
            //services.AddTransient<IFeatureHandler<CheckIdentifierRequest, CheckIdentifierResponse>, CheckIdentifierHandler>();

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
                options.AddPolicy("Admin", policy => policy.RequireRole("ADMIN"));
                options.AddPolicy("Moderator", policy => policy.RequireRole("MODERATOR"));
                options.AddPolicy("User", policy => policy.RequireRole("USER", "ADMIN", "MODERATOR"));
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