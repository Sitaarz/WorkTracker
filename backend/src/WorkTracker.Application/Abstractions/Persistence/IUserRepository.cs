using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Application.Abstractions.Authentication;
public interface IUserRepository
{
    Task CreateUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
}
