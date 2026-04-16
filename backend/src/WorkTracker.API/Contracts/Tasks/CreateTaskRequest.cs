using WorkTracker.Domain.Entities;

namespace WorkTracker.API.Contracts.Tasks;

public record CreateTaskRequest(
    string Title,
    string Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate);