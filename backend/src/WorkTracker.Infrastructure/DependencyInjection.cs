using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace WorkTracker.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register infrastructure services here
        // e.g., services.AddScoped<IMyRepository, MyRepository>();

        return services;
    }
}
