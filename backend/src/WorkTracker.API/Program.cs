using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Infrastructure.DependencyInjection;
using WorkTracker.Infrastructure.Persistence;
using WorkTracker.Application.DependencyInjection;
using WorkTracker.API.MiddleWare;

var builder = WebApplication.CreateBuilder(args);

// When running behind a reverse proxy (ingress-nginx, docker, etc.) trust the
// X-Forwarded-* headers so Request.IsHttps / Request.Scheme reflect the original
// client request. Without this the auth cookie would never be flagged Secure
// behind a TLS-terminating proxy, and secure-only cookies would be dropped.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Trust forwarded headers from any proxy in front of us (ingress / docker
    // internal networks are not predictable). In a hardened setup you would
    // restrict this to known proxy IP ranges.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

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

// When launched with `--migrate`, apply EF Core migrations and exit without starting the web host.
// Used by the Kubernetes migration Job to run schema updates as a separate, idempotent step.
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WorkTrackerDbContext>();
    await db.Database.MigrateAsync();
    return;
}

// Forwarded headers must run before any middleware that inspects the request
// scheme/host (exception handler, CORS, auth cookie creation, etc.).
app.UseForwardedHeaders();

// Global exception handling middleware
app.UseExceptionHandler();

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


// This is used by the integration tests to find the program class.
public partial class Program
{
}