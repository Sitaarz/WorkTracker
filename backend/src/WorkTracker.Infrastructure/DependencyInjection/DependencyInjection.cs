using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Application.Abstractions.Authentication;
using WorkTracker.Infrastructure.Authentication;
using WorkTracker.Infrastructure.Persistence;
using WorkTracker.Infrastructure.Persistence.Repositories;

namespace WorkTracker.Infrastructure.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<WorkTrackerDbContext>(Options => Options.UseSqlite(connectionString));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
