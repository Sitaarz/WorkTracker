using TaskPriorityEnum = WorkTracker.Infrastructure.Entities.TaskPriority;
using TaskStatusEnum = WorkTracker.Infrastructure.Entities.TaskItemStatus;

namespace WorkTracker.Application.Tasks.Update;

public sealed record class UpdateTaskCommand(
    Guid Id,
    string Title,
    string Description,
    TaskStatusEnum Status,
    TaskPriorityEnum Priority,
    DateTime? DueDate,
    Guid OwnerId,
    DateTime CreatedAt
);
