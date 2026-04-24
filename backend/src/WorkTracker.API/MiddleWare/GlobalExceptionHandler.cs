using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WorkTracker.API.MiddleWare;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception for request {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var statusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        // In production we return a generic ProblemDetails to avoid leaking
        // internal exception information (messages, types, etc.) to clients.
        var problem = _environment.IsProduction()
            ? BuildGenericProblem(statusCode, httpContext)
            : BuildDetailedProblem(statusCode, exception, httpContext);

        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static ProblemDetails BuildGenericProblem(int statusCode, HttpContext httpContext)
    {
        var (title, detail) = statusCode switch
        {
            StatusCodes.Status400BadRequest => ("Bad request", "The request was invalid."),
            StatusCodes.Status404NotFound => ("Not found", "The requested resource was not found."),
            StatusCodes.Status409Conflict => ("Conflict", "The request could not be completed due to a conflict."),
            _ => ("Server error", "An unexpected error occurred. Please try again later.")
        };

        return new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = httpContext.Request.Path
        };
    }

    private static ProblemDetails BuildDetailedProblem(int statusCode, Exception exception, HttpContext httpContext)
    {
        return new ProblemDetails
        {
            Title = "Server error",
            Detail = exception.Message,
            Status = statusCode,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = httpContext.Request.Path
        };
    }
}
