using Microsoft.EntityFrameworkCore;
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

    public async Task<bool> TryDeleteTaskAsync(Guid taskId)
    {
        var task = await _dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task != null)
        {
            _dbContext.TaskItems.Remove(task);
            await _dbContext.SaveChangesAsync();
        }
        return false;
    }

    public async Task<IEnumerable<TaskItem>> GetAllUserTasksAsync(Guid userId)
    {
        return await _dbContext.TaskItems
            .Where(task => task.Owner.Id == userId)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(Guid taskId)
    {
        return await _dbContext.TaskItems
            .FirstOrDefaultAsync(task => task.Id == taskId);
    }

    public async Task<bool> TryUpdateTaskAsync(TaskItem taskItem)
    {
        var existingTask = await _dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskItem.Id);
        if (existingTask == null)
        {
            return false;
        }

        _dbContext.TaskItems.Update(taskItem);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
