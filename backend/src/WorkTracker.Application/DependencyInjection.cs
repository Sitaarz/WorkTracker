using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace WorkTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Register application services here
        // e.g., services.AddScoped<IMyService, MyService>();

        return services;
    }
}
