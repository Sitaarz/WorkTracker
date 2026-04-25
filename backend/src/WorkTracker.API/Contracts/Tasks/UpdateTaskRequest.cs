using WorkTracker.Domain.Entities;

namespace WorkTracker.API.Contracts.Tasks;

public record UpdateTaskRequest(
    Guid Id,
    string Title,
    string Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate);
