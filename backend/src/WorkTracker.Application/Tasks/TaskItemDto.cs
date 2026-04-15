using WorkTracker.Domain.Entities;

namespace WorkTracker.Application.Tasks;

public sealed record TaskItemDto(
    Guid Id,
    string Title,
    string Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    Guid OwnerId,
    DateTime CreatedAt
);
