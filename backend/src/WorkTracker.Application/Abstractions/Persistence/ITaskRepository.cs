using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Application.Abstractions.Persistence;

public interface ITaskRepository
{
    Task CreateTaskAsync(TaskItem taskItem);
}
