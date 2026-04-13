using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTracker.API.Contracts.Tasks;
using WorkTracker.Application.Tasks;
using WorkTracker.Application.Tasks.Create;
using WorkTracker.Application.Tasks.Delete;
using WorkTracker.Application.Tasks.Get;
using WorkTracker.Application.Tasks.Get.All;
using WorkTracker.Application.Tasks.Get.Single;
using WorkTracker.Application.Tasks.Update;

namespace WorkTracker.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskCommandHandler _taskCommandHandler;
    private readonly ILogger<TasksController> _logger;

    public TasksController(TaskCommandHandler taskCommandHandler, ILogger<TasksController> logger)
    {
        _taskCommandHandler = taskCommandHandler;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand request)
    {
        var validationResult = new CreateTaskCommandValidator().Validate(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            return ValidationProblem(new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Errors = errors
            });
        }

        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Authenticated create task request is missing a valid subject claim.");

            return Problem(
                title: "Unauthorized",
                detail: "Required user claims were not found.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await _taskCommandHandler.Handle(request, userId);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Task creation failed for user {UserId}. Error: {ErrorMessage}", userId, result.ErrorMessage);

            return Problem(
                title: "Task creation failed",
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;

        var result = await _taskCommandHandler.Handle(new GetAllUserTasksCommand(Guid.Parse(userIdClaim)));
        return Ok(result);
    }

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetById([FromRoute] GetTaskCommand command)
    {
        _logger.LogInformation("Received request to retrieve task with ID {TaskId} using user {UserId}", command.TaskId, User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var result = await _taskCommandHandler.Handle(command);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Task retrieval failed for TaskId {TaskId}. Error: {ErrorMessage}", command.TaskId, result.ErrorMessage);
            return Problem(
                title: "Task retrieval failed",
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status404NotFound);
        }

        var usrIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;
        if (result?.Value?.OwnerId.ToString() != usrIdClaim)
        {
            _logger.LogWarning("Unauthorized access attempt to TaskId {TaskId} by UserId {UserId}", command.TaskId, usrIdClaim);
            return Problem(
                title: "Unauthorized",
                detail: "You do not have permission to access this task.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        return Ok(result.Value);
    }

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid taskId, [FromBody] UpdateTaskCommand updateTaskCommand)
    {
        if(taskId != updateTaskCommand.Id)
        {
            return Problem(
                title: "Bad Request",
                detail: "Task ID in the URL does not match Task ID in the body.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;

        if(updateTaskCommand.OwnerId.ToString() != userId)
        {
            _logger.LogWarning("Unauthorized update attempt to TaskId {TaskId} by UserId {UserId}", taskId, userId);
            return Problem(
                title: "Unauthorized",
                detail: "You do not have permission to update this task.",
                statusCode: StatusCodes.Status403Forbidden);
        }
        if (!await _taskCommandHandler.Handle(updateTaskCommand))
        {
            _logger.LogWarning("Task update failed for TaskId {TaskId} by UserId {UserId}. Error: {ErrorMessage}", taskId, userId, "Task does not exist or could not be updated.");

            return Problem(
                title: "Task update failed",
                detail: "Task does not exist or could not be updated.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok();
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid taskId)
    {
        _logger.LogInformation("Received request to delete task with ID {TaskId} using user {UserId}", taskId, User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;

        var existingTaskResult = await _taskCommandHandler.Handle(new DeleteTaskCommand(taskId, Guid.Parse(userId)));
        if (!existingTaskResult.IsSuccess)
        {
            if (existingTaskResult.ErrorMessage == "Task not found.")
            {
                _logger.LogWarning("Task with ID {TaskId} not found for deletion by UserId {UserId}", taskId, userId);
                return Problem(
                    title: "Task not found",
                    detail: existingTaskResult.ErrorMessage,
                    statusCode: StatusCodes.Status404NotFound);
            }

            if (existingTaskResult.ErrorMessage == "You do not have permission to delete this task.")
            {
                _logger.LogWarning("Unauthorized delete attempt to TaskId {TaskId} by UserId {UserId}", taskId, userId);
                return Problem(
                    title: "Unauthorized",
                    detail: existingTaskResult.ErrorMessage,
                    statusCode: StatusCodes.Status403Forbidden);
            }
        }
        return NoContent();
    }

    [HttpGet("filter")]
    public async Task<IActionResult> GetFiltered([FromQuery] GetTasksRequest request)
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;

        var query = new GetTaskQuery(
            OwnerId: Guid.Parse(userId),
            Status: request.Status,
            Priority: request.Priority,
            SortedBy: request.SortedBy,
            SortDirection: request.SortDirection,
            Page: request.Page,
            PageSize: request.PageSize
        );

        var result = await _taskCommandHandler.Handle(query);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to retrieve tasks for UserId {UserId} with filters. Error: {ErrorMessage}", userId, result.ErrorMessage);
            return Problem(
                title: "Failed to retrieve tasks",
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(result.Value);
    }
}
