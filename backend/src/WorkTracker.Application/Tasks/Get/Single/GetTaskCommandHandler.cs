using System;
using WorkTracker.Application.Abstractions.Authentication;
using WorkTracker.Application.Abstractions.Persistence;
using WorkTracker.Application.Common;

namespace WorkTracker.Application.Tasks.Get.Single;

public class GetTaskCommandHandler
{
    ITaskRepository _taskRepository;

    public GetTaskCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskItemDto>> HandleAsync(GetTaskCommand command)
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
