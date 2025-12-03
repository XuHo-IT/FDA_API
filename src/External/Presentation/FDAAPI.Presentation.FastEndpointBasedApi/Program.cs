using FastEndpoints;
using FastEndpoints.Swagger;
using FDAAPI.Infra.Configuration;
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

app.Run();
