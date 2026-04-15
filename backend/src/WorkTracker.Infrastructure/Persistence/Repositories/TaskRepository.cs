using Microsoft.EntityFrameworkCore;
using WorkTracker.Application.Abstractions.Persistence;
using WorkTracker.Application.Common;
using WorkTracker.Application.Tasks.Get;
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

    public async Task<PageResult<TaskItem>> QueryTasksAsync(GetTaskQuery query)
    {
        var queryItems =
        _dbContext.TaskItems
        .AsNoTracking()
        .Where(t=>t.OwnerId == query.OwnerId);

        if (query.Status is not null)
        {
            queryItems = queryItems.Where(t=>t.Status == query.Status);
        }

        if(query.Priority is not null)
        {
            queryItems = queryItems.Where(t=>t.Priority == query.Priority);
        }

        queryItems = SortTasks(queryItems, query);

        var totalCount = await queryItems.CountAsync();

        var items = await queryItems
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        return new PageResult<TaskItem>(items, totalCount, query.Page, query.PageSize);
    }

    private IQueryable<TaskItem> SortTasks(IQueryable<TaskItem> queryItems, GetTaskQuery query)
    {
        return (query.SortedBy, query.SortDirection) switch
        {
            (SortBy.CreatedAt, SortDirection.Asc) => queryItems.OrderBy(t => t.CreatedAt),
            (SortBy.CreatedAt, SortDirection.Desc) => queryItems.OrderByDescending(t => t.CreatedAt),
            (SortBy.DueDate, SortDirection.Asc) => queryItems.OrderBy(t => t.DueDate),
            (SortBy.DueDate, SortDirection.Desc) => queryItems.OrderByDescending(t => t.DueDate),
            _ => queryItems.OrderBy(t => t.CreatedAt)
        };
    }
}
