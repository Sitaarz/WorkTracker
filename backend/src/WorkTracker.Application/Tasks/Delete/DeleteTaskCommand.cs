namespace WorkTracker.Application.Tasks.Delete;

public record class DeleteTaskCommand(Guid TaskId, Guid UserId);
