namespace WorkTracker.Application.Tasks.Update;

public sealed record class UpdateTaskCommand(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid OwnerId,
    DateTime CreatedAt
);
