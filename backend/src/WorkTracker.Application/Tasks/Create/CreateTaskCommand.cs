namespace WorkTracker.Application.Tasks.Create;

public sealed record class CreateTaskCommand(
    string Title,
    string Description,
    string Status,
    string Priority,
    DateTime? DueDate);
