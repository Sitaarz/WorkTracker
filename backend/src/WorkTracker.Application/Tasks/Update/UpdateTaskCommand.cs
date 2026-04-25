using TaskPriorityEnum = WorkTracker.Domain.Entities.TaskPriority;
using TaskStatusEnum = WorkTracker.Domain.Entities.TaskItemStatus;

namespace WorkTracker.Application.Tasks.Update;

public sealed record class UpdateTaskCommand(
    Guid Id,
    Guid UserId,
    string Title,
    string Description,
    TaskStatusEnum Status,
    TaskPriorityEnum Priority,
    DateTime? DueDate
);
