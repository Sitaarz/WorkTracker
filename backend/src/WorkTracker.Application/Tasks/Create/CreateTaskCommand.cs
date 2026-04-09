using TaskPriorityEnum = WorkTracker.Infrastructure.Entities.TaskPriority;
using TaskStatusEnum = WorkTracker.Infrastructure.Entities.TaskStatus;

namespace WorkTracker.Application.Tasks.Create;

public sealed record class CreateTaskCommand(
    string Title,
    string Description,
    TaskStatusEnum Status,
    TaskPriorityEnum Priority,
    DateTime? DueDate);
