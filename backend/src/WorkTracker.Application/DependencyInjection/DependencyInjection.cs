using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using WorkTracker.Infrastructure.Entities;
using WorkTracker.Application.Authentication.Register;
using WorkTracker.Application.Authentication.Login;
using WorkTracker.Application.Tasks;

namespace WorkTracker.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LogInUserHandler>();
        services.AddScoped<TaskCommandHandler>();

        return services;
    }
}
