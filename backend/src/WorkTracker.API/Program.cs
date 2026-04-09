using System.Text.Json.Serialization;
using WorkTracker.Infrastructure.DependencyInjection;
using WorkTracker.Application.DependencyInjection;
using WorkTracker.API.MiddleWare;

var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddHealthChecks();

var app = builder.Build();

// Global exception handling middleware
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Enable OpenAPI/Swagger in development environment
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "WorkTracker API");
    });
}

// Redirect http to https
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health").WithName("HealthCheck");

app.Run();
