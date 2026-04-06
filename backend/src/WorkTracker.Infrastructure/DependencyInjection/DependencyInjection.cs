using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Infrastructure.Persistence;

namespace WorkTracker.Infrastructure.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<WorkTrackerDbContext>(Options => Options.UseSqlite(connectionString));
        // Register infrastructure services here
        // e.g., services.AddScoped<IMyRepository, MyRepository>();

        return services;
    }
}
