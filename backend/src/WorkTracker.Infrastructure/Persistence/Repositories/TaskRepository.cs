using WorkTracker.Application.Abstractions.Persistence;
using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly WorkTrackerDbContext _dbContext;

    public TaskRepository(WorkTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateTaskAsync(TaskItem taskItem)
    {
        if (taskItem.CreatedAt == default)
        {
            taskItem.CreatedAt = DateTime.UtcNow;
        }

        _dbContext.TaskItems.Add(taskItem);
        await _dbContext.SaveChangesAsync();
    }
}
