using Microsoft.Extensions.DependencyInjection;
using FDAAPI.App.Common.Features;
using FDAAPI.App.Feat1;
using FDAAPI.App.Feat2;
using FDAAPI.App.Feat3;
using FDAAPI.App.Feat4;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Infra.Persistence.Repositories;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using FDAAPI.App.FeatG5;

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
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IFeatureHandler<CreateWaterLevelRequest, CreateWaterLevelResponse>, CreateWaterLevelHandler>();

            services.AddTransient<IFeatureHandler<UpdateWaterLevelRequest, UpdateWaterLevelResponse>, UpdateWaterLevelHandler>();

            services.AddTransient<IFeatureHandler<GetWaterLevelRequest, GetWaterLevelResponse>, GetWaterLevelHandler>();

            services.AddTransient<IFeatureHandler<DeleteWaterLevelRequest, DeleteWaterLevelResponse>, DeleteWaterLevelHandler>();
            services.AddTransient<IFeatureHandler<GetStaticDataRequest, GetStaticDataResponse>, GetStaticDataHandler>();


            return services;
        }
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
{
    // Ưu tiên lấy từ biến môi trường Docker (ConnectionStrings__PostgreSQLConnection)
    var connectionString = configuration.GetConnectionString("PostgreSQLConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection String undefined!");
    }

    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    services.AddScoped<IWaterLevelRepository, PgsqlWaterLevelRepository>();

    return services;
}
    }
}