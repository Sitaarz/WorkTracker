using Microsoft.EntityFrameworkCore;
using WorkTracker.Application.Abstractions.Authentication;
using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WorkTrackerDbContext _dbContext;

    public UserRepository(WorkTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateUserAsync(User user)
    {
        var utcNow = DateTime.UtcNow;

        if (user.CreatedAt == default)
        {
            user.CreatedAt = utcNow;
        }

        user.UpdatedAt = utcNow;

        _dbContext.Set<User>().Add(user);
        await _dbContext.SaveChangesAsync();
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        return _dbContext
            .Set<User>()
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == email);
    }
}
