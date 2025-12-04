using FastEndpoints;
using FastEndpoints.Swagger;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Infra.Configuration;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices()
    .AddPersistenceServices(builder.Configuration);

// Add FastEndpoints
builder.Services.AddFastEndpoints().SwaggerDocument(); 

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapScalarApiReference(options =>
{
    options.Title = "FDA API Documentation";
});
app.UseHttpsRedirection();
app.UseFastEndpoints()
   .UseSwaggerGen();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Lệnh này áp dụng các migration đang chờ xử lý (tạo bảng)
        context.Database.Migrate();
        Console.WriteLine("Database Migrated Successfully.");
    }
    catch (Exception ex)
    {
        // Ghi lại lỗi nếu migration thất bại (ví dụ: lỗi kết nối DB)
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();
