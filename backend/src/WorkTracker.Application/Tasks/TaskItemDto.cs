using TaskPriorityEnum = WorkTracker.Infrastructure.Entities.TaskPriority;
using TaskStatusEnum = WorkTracker.Infrastructure.Entities.TaskStatus;

namespace WorkTracker.Application.Tasks;

public sealed record TaskItemDto(
    Guid Id,
    string Title,
    string Description,
    TaskStatusEnum Status,
    TaskPriorityEnum Priority,
    DateTime? DueDate,
    Guid OwnerId,
    DateTime CreatedAt
);
