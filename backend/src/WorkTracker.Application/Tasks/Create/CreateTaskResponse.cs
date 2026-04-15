using TaskPriorityEnum = WorkTracker.Domain.Entities.TaskPriority;
using TaskStatusEnum = WorkTracker.Domain.Entities.TaskItemStatus;

namespace WorkTracker.Application.Tasks.Create;

public sealed record class CreateTaskResponse(
    Guid Id,
    string Title,
    string Description,
    TaskStatusEnum Status,
    TaskPriorityEnum Priority,
    DateTime? DueDate,
    Guid OwnerId,
    DateTime CreatedAt);
