namespace WorkTracker.Application.Tasks.Create;

public sealed record class CreateTaskResponse(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid OwnerId,
    DateTime CreatedAt);
