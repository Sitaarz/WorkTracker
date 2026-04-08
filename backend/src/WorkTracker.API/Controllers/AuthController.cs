using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkTracker.Application.Authentication.Login;
using WorkTracker.Application.Authentication.Register;

namespace WorkTracker.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUserHandler _registerUserHandler;
        private readonly LogInUserHandler _loginUserHandler;
        private readonly ILogger<AuthController> _logger;
        public AuthController(RegisterUserHandler registerUserHandler, LogInUserHandler loginUserHandler, ILogger<AuthController> logger)
        {
            _registerUserHandler = registerUserHandler;
            _loginUserHandler = loginUserHandler;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand request)
        {
            _logger.LogInformation("Received registration request for email: {Email}", request.Email);
            var validationResult = new RegisterUserCommandValidator().Validate(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Registration request for email: {Email} failed validation.", request.Email);
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

            var result = await _registerUserHandler.HandleAsync(request);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Registration failed for email: {Email}. Error: {ErrorMessage}", request.Email, result.ErrorMessage);
                return Problem(
                    title: "Registration Failed",
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status409Conflict
                );
            }

            return Ok(result.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LogInUserCommand request)
        {
            _logger.LogInformation("Received login request for email: {Email}", request.Email);
            var validationResult = new LoginUserCommandValidator().Validate(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Login request for email: {Email} failed validation.", request.Email);
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

            var result = await _loginUserHandler.HandleAsync(request);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login failed for email: {Email}. Error: {ErrorMessage}", request.Email, result.ErrorMessage);
                return Problem(
                    title: "Login Failed",
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }

            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var userNameClaim = User.FindFirst(JwtRegisteredClaimNames.Name)?.Value;
            var userEmailClaim = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                string.IsNullOrWhiteSpace(userNameClaim) ||
                string.IsNullOrWhiteSpace(userEmailClaim) ||
                string.IsNullOrWhiteSpace(userRoleClaim) ||
                !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Authenticated request is missing required JWT claims.");

                return Problem(
                    title: "Unauthorized",
                    detail: "Required user claims were not found.",
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }

            return Ok(new GetCurrentUserResponse(userId, userNameClaim, userEmailClaim, userRoleClaim));
        }
        private record GetCurrentUserResponse(Guid UserId, string Name, string Email, string Role);
    }
}
