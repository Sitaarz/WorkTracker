using WorkTracker.Application.Abstractions.Persistence;
using WorkTracker.Application.Common;
using WorkTracker.Application.Tasks.Create;
using WorkTracker.Application.Tasks.Delete;
using WorkTracker.Application.Tasks.Get;
using WorkTracker.Application.Tasks.Get.All;
using WorkTracker.Application.Tasks.Get.Single;
using WorkTracker.Application.Tasks.Update;
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
            Status = command.Status,
            Priority = command.Priority,
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

    public async Task<bool> Handle(UpdateTaskCommand command)
    {
        var existingTask = await _taskRepository.GetTaskByIdAsync(command.Id);
        if (existingTask == null)
        {
            return false;
        }

        existingTask.Title = command.Title.Trim();
        existingTask.Description = command.Description.Trim();
        existingTask.Status = command.Status;
        existingTask.Priority = command.Priority;
        existingTask.DueDate = command.DueDate;

        return await _taskRepository.TryUpdateTaskAsync(existingTask);
    }

    public async Task<Result> Handle(DeleteTaskCommand command)
    {
        var taskItem = await _taskRepository.GetTaskByIdAsync(command.TaskId);
        if (taskItem == null)        {
            return Result.Failure("Task not found.");
        }
        if (taskItem.OwnerId != command.UserId)
        {
            return Result.Failure("You do not have permission to delete this task.");
        }

        var result = await _taskRepository.TryDeleteTaskAsync(command.TaskId);
        if (!result)
        {
            return Result.Failure("Failed to delete task.");
        }

        return Result.Success();
    }

    public async Task<Result<PageResult<TaskItemDto>>> Handle(GetTaskQuery query)
    {
        if(query.Page <= 0 || query.PageSize <= 0)
        {
            return Result<PageResult<TaskItemDto>>.Failure("Page and PageSize must be greater than 0.");
        }
        var pageResult = await _taskRepository.QueryTasksAsync(query);

        var taskDtos = pageResult.Items.Select(t => new TaskItemDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            t.DueDate,
            t.OwnerId,
            t.CreatedAt
        )).ToList();

        return Result<PageResult<TaskItemDto>>.Success(new PageResult<TaskItemDto>(
            taskDtos,
            pageResult.TotalCount,
            pageResult.Page,
            pageResult.PageSize)
        );
    }
}
