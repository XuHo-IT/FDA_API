using FDAAPI.App.Common.Behaviors;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Services;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.App.FeatG1_SensorReadingCreate;
using FDAAPI.App.FeatG10_AuthChangePassword;
using FDAAPI.App.FeatG11_AuthSetPassword;
using FDAAPI.App.FeatG12_AuthGoogleLoginInitiate;
using FDAAPI.App.FeatG13_AuthGoogleOAuthCallback;
using FDAAPI.App.FeatG14_ProfileGet;
using FDAAPI.App.FeatG15_ProfileUpdate;
using FDAAPI.App.FeatG16_AuthGoogleMobileLogin;
using FDAAPI.App.FeatG17_AuthCheckIdentifier;
using FDAAPI.App.FeatG18_MediaUploadImage;
using FDAAPI.App.FeatG19_ProfileVerifyUpdatePhone;
using FDAAPI.App.FeatG2_SensorReadingUpdate;
using FDAAPI.App.FeatG20_UserCreate;
using FDAAPI.App.FeatG20_UserCreate;
using FDAAPI.App.FeatG21_UserList;
using FDAAPI.App.FeatG21_UserList;
using FDAAPI.App.FeatG22_UserUpdate;
using FDAAPI.App.FeatG22_UserUpdate;
using FDAAPI.App.FeatG23_StationCreate;
using FDAAPI.App.FeatG23_StationCreate;
using FDAAPI.App.FeatG24_StationUpdate;
using FDAAPI.App.FeatG24_StationUpdate;
using FDAAPI.App.FeatG25_StationList;
using FDAAPI.App.FeatG25_StationList;
using FDAAPI.App.FeatG26_StationGet;
using FDAAPI.App.FeatG26_StationGet;
using FDAAPI.App.FeatG27_StationDelete;
using FDAAPI.App.FeatG27_StationDelete;
using FDAAPI.App.FeatG28_GetMapPreferences;
using FDAAPI.App.FeatG28_GetMapPreferences;
using FDAAPI.App.FeatG29_UpdateMapPreferences;
using FDAAPI.App.FeatG29_UpdateMapPreferences;
using FDAAPI.App.FeatG3_SensorReadingGet;
using FDAAPI.App.FeatG30_GetFloodSeverityLayer;
using FDAAPI.App.FeatG30_GetFloodSeverityLayer;
using FDAAPI.App.FeatG31_GetMapCurrentStatus;
using FDAAPI.App.FeatG31_GetMapCurrentStatus;
using FDAAPI.App.FeatG32_AreaCreate;
using FDAAPI.App.FeatG32_AreaCreate;
using FDAAPI.App.FeatG33_AreaListByUser;
using FDAAPI.App.FeatG33_AreaListByUser;
using FDAAPI.App.FeatG34_AreaStatusEvaluate;
using FDAAPI.App.FeatG34_AreaStatusEvaluate;
using FDAAPI.App.FeatG35_AreaGet;
using FDAAPI.App.FeatG35_AreaGet;
using FDAAPI.App.FeatG36_AreaUpdate;
using FDAAPI.App.FeatG36_AreaUpdate;
using FDAAPI.App.FeatG37_AreaDelete;
using FDAAPI.App.FeatG37_AreaDelete;
using FDAAPI.App.FeatG38_AreaList;
using FDAAPI.App.FeatG38_AreaList;
using FDAAPI.App.FeatG39_SubscribeToAlerts;
using FDAAPI.App.FeatG4_SensorReadingDelete;
using FDAAPI.App.FeatG40_GetAlertHistory;
using FDAAPI.App.FeatG41_UpdateAlertPreferences;
using FDAAPI.App.FeatG42_ProcessAlerts;
using FDAAPI.App.FeatG43_DispatchNotifications;
using FDAAPI.App.FeatG44_GetFloodHistory;
using FDAAPI.App.FeatG45_GetFloodTrends;
using FDAAPI.App.FeatG46_GetFloodStatistics;
using FDAAPI.App.FeatG47_FrequencyAggregation;
using FDAAPI.App.FeatG48_SeverityAggregation;
using FDAAPI.App.FeatG49_HotspotAggregation;
using FDAAPI.App.FeatG50_GetJobStatus;
using FDAAPI.App.FeatG51_GetFrequencyAnalytics;
using FDAAPI.App.FeatG52_GetSeverityAnalytics;
using FDAAPI.App.FeatG53_GetHotspotRankings;
using FDAAPI.App.FeatG57_AdministrativeAreaCreate;
using FDAAPI.App.FeatG58_AdministrativeAreaList;
using FDAAPI.App.FeatG59_AdministrativeAreaGet;
using FDAAPI.App.FeatG6_AuthSendOtp;
using FDAAPI.App.FeatG60_AdministrativeAreaUpdate;
using FDAAPI.App.FeatG61_AdministrativeAreaDelete;
using FDAAPI.App.FeatG62_FloodEventCreate;
using FDAAPI.App.FeatG63_FloodEventList;
using FDAAPI.App.FeatG64_FloodEventGet;
using FDAAPI.App.FeatG65_FloodEventUpdate;
using FDAAPI.App.FeatG66_FloodEventDelete;
using FDAAPI.App.FeatG67_GetMySubscriptions;
using FDAAPI.App.FeatG68_DeleteSubscription;
using FDAAPI.App.FeatG7_AuthLogin;
using FDAAPI.App.FeatG70_AdminGetAlertStats;
using FDAAPI.App.FeatG8_AuthRefreshToken;
using FDAAPI.App.FeatG9_AuthLogout;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Infra.Persistence.Repositories;
using FDAAPI.Infra.Services.Aggregation;
using FDAAPI.Infra.Services.Alerts;
using FDAAPI.Infra.Services.Auth;
using FDAAPI.Infra.Services.Cache;
using FDAAPI.Infra.Services.Notifications;
using FDAAPI.Infra.Services.OAuth;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Quartz;
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
            
            // Mappers
            services.AddScoped<IUserProfileMapper, UserProfileMapper>();
            services.AddScoped<IUserMapper, UserMapper>();
            services.AddScoped<IStationMapper, StationMapper>();
            services.AddScoped<ISensorReadingMapper, SensorReadingMapper>();
            services.AddScoped<IAreaMapper, AreaMapper>();
            services.AddScoped<IFloodHistoryMapper, FloodHistoryMapper>();
            services.AddScoped<IAdministrativeAreaMapper, AdministrativeAreaMapper>();
            services.AddScoped<IFloodEventMapper, FloodEventMapper>();
            services.AddScoped<IGlobalThresholdService, GlobalThresholdService>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assemblies = new[]
            {
                typeof(VerifyAndUpdatePhoneRequest).Assembly,
                typeof(SendOtpRequest).Assembly,
                typeof(LoginRequest).Assembly,
                typeof(LogoutRequest).Assembly,
                typeof(RefreshTokenRequest).Assembly,
                typeof(ChangePasswordRequest).Assembly,
                typeof(GoogleLoginInitiateRequest).Assembly,
                typeof(GoogleOAuthCallbackRequest).Assembly,
                typeof(GoogleMobileLoginRequest).Assembly,
                typeof(GetProfileRequest).Assembly,
                typeof(SetPasswordRequest).Assembly,
                typeof(CheckIdentifierRequest).Assembly,
                typeof(CreateUserRequest).Assembly,
                typeof(GetUsersRequest).Assembly,
                typeof(UpdateUserRequest).Assembly,
                typeof(UpdateProfileRequest).Assembly,
                typeof(CreateStationRequest).Assembly,
                typeof(UpdateStationRequest).Assembly,
                typeof(GetStationRequest).Assembly,
                typeof(GetStationsRequest).Assembly,
                typeof(DeleteStationRequest).Assembly,
                typeof(GetMapPreferencesRequest).Assembly,
                typeof(UpdateMapPreferencesRequest).Assembly,
                typeof(GetFloodSeverityLayerRequest).Assembly,
                typeof(CreateSensorReadingRequest).Assembly,
                typeof(UpdateSensorReadingRequest).Assembly,
                typeof(GetSensorReadingRequest).Assembly,
                typeof(DeleteSensorReadingRequest).Assembly,
                typeof(GetMapCurrentStatusRequest).Assembly,
                typeof(CreateAreaRequest).Assembly,
                typeof(AreaListByUserRequest).Assembly,
                typeof(AreaStatusEvaluateRequest).Assembly,
                typeof(AreaGetRequest).Assembly,
                typeof(AreaListRequest).Assembly,
                typeof(UpdateAreaRequest).Assembly,
                typeof(DeleteAreaRequest).Assembly,
                typeof(SubscribeToAlertsRequest).Assembly,
                typeof(GetAlertHistoryRequest).Assembly,
                typeof(UpdateAlertPreferencesRequest).Assembly,
                typeof(ProcessAlertsRequest).Assembly,
                typeof(DispatchNotificationsRequest).Assembly,
                typeof(GetFloodHistoryRequest).Assembly,
                typeof(GetFloodTrendsRequest).Assembly,
                typeof(GetFloodStatisticsRequest).Assembly,
                typeof(FrequencyAggregationRequest).Assembly,
                typeof(SeverityAggregationRequest).Assembly,
                typeof(HotspotAggregationRequest).Assembly,
                typeof(GetJobStatusRequest).Assembly,
                typeof(GetFrequencyAnalyticsRequest).Assembly,
                typeof(GetSeverityAnalyticsRequest).Assembly,
                typeof(GetHotspotRankingsRequest).Assembly,
                typeof(CreateAdministrativeAreaRequest).Assembly,
                typeof(GetAdministrativeAreasRequest).Assembly,
                typeof(GetAdministrativeAreaRequest).Assembly,
                typeof(UpdateAdministrativeAreaRequest).Assembly,
                typeof(DeleteAdministrativeAreaRequest).Assembly,
                typeof(CreateFloodEventRequest).Assembly,
                typeof(GetFloodEventsRequest).Assembly,
                typeof(GetFloodEventRequest).Assembly,
                typeof(UpdateFloodEventRequest).Assembly,
                typeof(DeleteFloodEventRequest).Assembly,
                typeof(GetFloodStatisticsRequest).Assembly,
                typeof(GetMySubscriptionsRequest).Assembly,
                typeof(DeleteSubscriptionRequest).Assembly,
                typeof(AdminGetAlertStatsRequest).Assembly,
            };

            // Register MediatR with all feature assemblies and ValidationBehavior
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assemblies);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // Register all FluentValidation validators from all feature assemblies
            services.AddValidatorsFromAssemblies(assemblies);

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

            services.AddScoped<ISensorReadingRepository, PgsqlSensorReadingRepository>();
            services.AddScoped<IStationRepository, PgslStationRepository>();
            services.AddScoped<IUserRepository, PgsqlUserRepository>();
            services.AddScoped<IRoleRepository, PgsqlRoleRepository>();
            services.AddScoped<IUserRoleRepository, PgsqlUserRoleRepository>();
            services.AddScoped<IRefreshTokenRepository, PgsqlRefreshTokenRepository>();
            services.AddScoped<IOtpCodeRepository, PgsqlOtpCodeRepository>();

            services.AddScoped<IAlertRepository, PgsqlAlertRepository>();
            services.AddScoped<IAlertRuleRepository, PgsqlAlertRuleRepository>();
            services.AddScoped<INotificationLogRepository, PgsqlNotificationLogRepository>();
            services.AddScoped<IUserAlertSubscriptionRepository, PgsqlUserAlertSubscriptionRepository>();
            services.AddScoped<IPriorityRoutingService, PriorityRoutingService>();
            services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
            services.AddScoped<INotificationDispatchService, NotificationDispatchService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IAlertCooldownConfigRepository, PgsqlAlertCooldownConfigRepository>();

            // OAuth provider repository
            services.AddScoped<IUserOAuthProviderRepository, PgsqlUserOAuthProviderRepository>();

            services.AddScoped<IUserPreferenceRepository, PgsqlUserPreferenceRepository>();

            services.AddScoped<IAreaRepository, PgsqlAreaRepository>();

            services.AddScoped<ISensorHourlyAggRepository, PgsqlSensorHourlyAggRepository>();
            services.AddScoped<ISensorDailyAggRepository, PgsqlSensorDailyAggRepository>();

            // Analytics repositories
            services.AddScoped<IAdministrativeAreaRepository, PgsqlAdministrativeAreaRepository>();
            services.AddScoped<IFloodAnalyticsFrequencyRepository, PgsqlFloodAnalyticsFrequencyRepository>();
            services.AddScoped<IFloodAnalyticsSeverityRepository, PgsqlFloodAnalyticsSeverityRepository>();
            services.AddScoped<IFloodAnalyticsHotspotRepository, PgsqlFloodAnalyticsHotspotRepository>();
            services.AddScoped<IAnalyticsJobRepository, PgsqlAnalyticsJobRepository>();
            services.AddScoped<IAnalyticsJobRunRepository, PgsqlAnalyticsJobRunRepository>();
            services.AddScoped<IFloodEventRepository, PgsqlFloodEventRepository>();

            // Analytics background job services (for Hangfire)
            services.AddScoped<FrequencyAggregationBackgroundJob>();
            services.AddScoped<SeverityAggregationBackgroundJob>();
            services.AddScoped<HotspotAggregationBackgroundJob>();

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

        public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
        {
            // Quartz for scheduled jobs
            services.AddQuartz(q =>
            {
                // Use a Scoped container for creating jobs
                q.UseMicrosoftDependencyInjectionJobFactory();

                // Hourly Aggregation Job - runs every hour at :05
                var hourlyJobKey = new JobKey("HourlyAggregationJob");
                q.AddJob<HourlyAggregationJob>(opts => opts.WithIdentity(hourlyJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(hourlyJobKey)
                    .WithIdentity("HourlyAggregationTrigger")
                    .WithCronSchedule("0 5 * * * ?")  // Every hour at :05
                    .WithDescription("Aggregates sensor readings into hourly summaries"));

                // Daily Aggregation Job - runs daily at 00:15 UTC
                var dailyJobKey = new JobKey("DailyAggregationJob");
                q.AddJob<DailyAggregationJob>(opts => opts.WithIdentity(dailyJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(dailyJobKey)
                    .WithIdentity("DailyAggregationTrigger")
                    .WithCronSchedule("0 15 0 * * ?")  // Daily at 00:15 UTC
                    .WithDescription("Aggregates hourly data into daily summaries"));
            });

            // Add Quartz hosted service
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            // Hangfire for on-demand background jobs (analytics aggregation)
            var connectionString = configuration.GetConnectionString("PostgreSQLConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING");
            }

            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddHangfire(config => config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(connectionString, new Hangfire.PostgreSql.PostgreSqlStorageOptions
                    {
                        SchemaName = "hangfire"
                    }));

                // Add the processing server as IHostedService
                services.AddHangfireServer(options =>
                {
                    options.WorkerCount = 5; // Number of concurrent workers
                    options.ServerTimeout = TimeSpan.FromMinutes(4);
                    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
                });
            }

            return services;
        }

    }
}

