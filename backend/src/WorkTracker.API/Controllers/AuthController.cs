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
        public AuthController(RegisterUserHandler registerUserHandler, LogInUserHandler loginUserHandler)
        {
            _registerUserHandler = registerUserHandler;
            _loginUserHandler = loginUserHandler;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand request)
        {
            var validationResult = new RegisterUserCommandValidator().Validate(request);
            if (!validationResult.IsValid)            {
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
            var validationResult = new LoginUserCommandValidator().Validate(request);
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

            var result = await _loginUserHandler.HandleAsync(request);
            if (!result.IsSuccess)
            {
                return Problem(
                    title: "Login Failed",
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }

            return Ok(result.Value);
        }
    }
}
