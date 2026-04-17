using WorkTracker.Domain.Entities;

namespace WorkTracker.Application.Abstractions.Authentication;

public interface IJwtGenerator
{
    string GenerateToken(User user);
}
