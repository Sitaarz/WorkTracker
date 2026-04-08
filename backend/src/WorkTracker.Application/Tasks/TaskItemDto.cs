namespace WorkTracker.Application.Tasks;

public sealed record TaskItemDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid OwnerId,
    DateTime CreatedAt
);
