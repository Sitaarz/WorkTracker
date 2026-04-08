using WorkTracker.Application.Abstractions.Persistence;
using WorkTracker.Application.Common;
using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Application.Tasks.Create;

public class CreateTaskHandler
{
    private readonly ITaskRepository _taskRepository;

    public CreateTaskHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<CreateTaskResponse>> HandleAsync(CreateTaskCommand command, Guid ownerId)
    {
        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            Description = command.Description.Trim(),
            Status = command.Status.Trim(),
            Priority = command.Priority.Trim(),
            DueDate = command.DueDate,
            OwnerId = ownerId
        };

        await _taskRepository.CreateTaskAsync(taskItem);

        return Result<CreateTaskResponse>.Success(new CreateTaskResponse(
            taskItem.Id,
            taskItem.Title,
            taskItem.Description,
            taskItem.Status,
            taskItem.Priority,
            taskItem.DueDate,
            taskItem.OwnerId,
            taskItem.CreatedAt));
    }
}
