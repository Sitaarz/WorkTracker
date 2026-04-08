using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using WorkTracker.Infrastructure.Entities;
using WorkTracker.Application.Authentication.Register;
using WorkTracker.Application.Authentication.Login;

namespace WorkTracker.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LogInUserHandler>();

        return services;
    }
}
