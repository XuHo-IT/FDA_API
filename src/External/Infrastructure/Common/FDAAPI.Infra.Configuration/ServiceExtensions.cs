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

namespace FDAAPI.Infra.Configuration
{
    public static class ServiceExtensions
    {
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

            return services;
        }
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection")));

            services.AddScoped<IWaterLevelRepository, PgsqlWaterLevelRepository>();

            return services;
        }
    }
}
