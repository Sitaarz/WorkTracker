using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTracker.Application.Tasks.Create;

namespace WorkTracker.API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly CreateTaskHandler _createTaskHandler;
    private readonly ILogger<TasksController> _logger;

    public TasksController(CreateTaskHandler createTaskHandler, ILogger<TasksController> logger)
    {
        _createTaskHandler = createTaskHandler;
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

        var result = await _createTaskHandler.HandleAsync(request, userId);
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
}
