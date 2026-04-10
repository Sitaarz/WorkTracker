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

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

// Trim and drop empty entries (e.g. from env var placeholders).
corsOrigins = corsOrigins
    .Select(o => o.Trim())
    .Where(o => o.Length > 0)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AppCors", policy =>
        {
            policy
                .WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}

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

// Redirect http to https
app.UseHttpsRedirection();

if (corsOrigins.Length > 0)
{
    // Must run before authentication when using cookie credentials from a SPA on another origin.
    app.UseCors("AppCors");
}

if (app.Environment.IsDevelopment())
{
    // Enable OpenAPI/Swagger in development environment
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "WorkTracker API");
    });
}



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health").WithName("HealthCheck");

app.Run();
