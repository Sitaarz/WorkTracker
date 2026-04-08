using WorkTracker.Application.Abstractions.Persistence;
using WorkTracker.Application.Common;
using WorkTracker.Application.Tasks.Create;
using WorkTracker.Application.Tasks.Get.All;
using WorkTracker.Application.Tasks.Get.Single;
using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Application.Tasks;

public class TaskCommandHandler
{
    private readonly ITaskRepository _taskRepository;

    public TaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<CreateTaskResponse>> Handle(CreateTaskCommand command, Guid ownerId)
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

    public async Task<IEnumerable<TaskItemDto>> Handle(GetAllUserTasksCommand command)
    {
        var tasks = await _taskRepository.GetAllUserTasksAsync(command.UserId);

        return tasks.Select(t => new TaskItemDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.DueDate,
            t.OwnerId,
            t.CreatedAt
        ));
    }

    public async Task<Result<TaskItemDto>> Handle(GetTaskCommand command)
    {
        var taskItem = await _taskRepository.GetTaskByIdAsync(Guid.Parse(command.TaskId));
        if (taskItem == null)
        {
            return Result<TaskItemDto>.Failure("Task not found.");
        }

        var taskItemDto = new TaskItemDto(
            taskItem.Id,
            taskItem.Title,
            taskItem.Description,
            taskItem.Status,
            taskItem.Priority,
            taskItem.DueDate,
            taskItem.OwnerId,
            taskItem.CreatedAt
        );

        return Result<TaskItemDto>.Success(taskItemDto);
    }
}
