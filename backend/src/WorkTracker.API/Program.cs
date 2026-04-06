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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    throw new InvalidOperationException("This is a test exception to demonstrate global error handling.");
})
.WithName("GetWeatherForecast");

app.MapHealthChecks("/health").WithName("HealthCheck");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
