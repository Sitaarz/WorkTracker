namespace WorkTracker.Application.Common;

public record class Result<T>(bool IsSuccess, T? Value = default, string? ErrorMessage = null)
{
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
}
