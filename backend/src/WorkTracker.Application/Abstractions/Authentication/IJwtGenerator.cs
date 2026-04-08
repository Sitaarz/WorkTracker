using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Application.Abstractions.Authentication;

public interface IJwtGenerator
{
    string GenerateToken(User user);
}
