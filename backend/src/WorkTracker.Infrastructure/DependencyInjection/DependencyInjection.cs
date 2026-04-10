using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkTracker.Application.Abstractions.Authentication;
using WorkTracker.Application.Abstractions.Persistence;
using WorkTracker.Infrastructure.Authentication;
using WorkTracker.Infrastructure.Persistence;
using WorkTracker.Infrastructure.Persistence.Repositories;

namespace WorkTracker.Infrastructure.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration is missing.");

        services.AddDbContext<WorkTrackerDbContext>(Options => Options.UseSqlite(connectionString));
        services.Configure<JwtOptions>(jwtSection);
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    NameClaimType = JwtRegisteredClaimNames.Name,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["access_token"];
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var userId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                            var email = context.Principal?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

                            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(email))
                            {
                                context.Fail("Required claims are missing.");
                            }

                            return Task.CompletedTask;
                        }
                    };
            });
        services.AddAuthorization();
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
