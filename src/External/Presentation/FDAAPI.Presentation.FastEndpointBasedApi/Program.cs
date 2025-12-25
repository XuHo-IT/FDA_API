using FastEndpoints;
using FastEndpoints.Swagger;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Infra.Configuration;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices()
    .AddPersistenceServices(builder.Configuration);


builder.Services.AddFastEndpoints().SwaggerDocument(); 


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {

        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
   
}
app.MapScalarApiReference(options =>
{
    options.Title = "FDA API Documentation";
});
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseFastEndpoints()
   .UseSwaggerGen();

using (var scope = app.Services.CreateScope())
{
   var services = scope.ServiceProvider;
   try
   {
       var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
       context.Database.Migrate();
       Console.WriteLine("Database Migrated Successfully.");

   }
   catch (Exception ex)
   {
       var logger = services.GetRequiredService<ILogger<Program>>();
       logger.LogError(ex, "An error occurred while migrating the database.");
   }
}

app.Run();
