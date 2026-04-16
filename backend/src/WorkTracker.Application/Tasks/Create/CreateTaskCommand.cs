using WorkTracker.Domain.Entities;

namespace WorkTracker.Application.Tasks.Create;

public sealed record class CreateTaskCommand(
    Guid OwnerId,
    string Title,
    string Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate);
