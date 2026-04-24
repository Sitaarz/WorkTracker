using WorkTracker.API.Contracts.Tasks;
using WorkTracker.Application.Tasks.Create;
using WorkTracker.Application.Tasks.Update;

namespace WorkTracker.API.Mappers;

public static class TasksMappers
{
    public static CreateTaskCommand ToCommand(this CreateTaskRequest request, Guid ownerId)
    {
        return new CreateTaskCommand(
            ownerId,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate);
    }

    public static UpdateTaskCommand ToCommand(this UpdateTaskRequest request, Guid userId)
    {
        return new UpdateTaskCommand(
            request.Id,
            userId,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate);
    }
}
