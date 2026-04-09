namespace WorkTracker.Application.Common;

public record class Result<T>(bool IsSuccess, T? Value = default, string? ErrorMessage = null)
{
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
}

public sealed record Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, string? errorMessage)
    {
        if (isSuccess && errorMessage is not null)
            throw new ArgumentException("Successful result cannot contain an error message.");

        if (!isSuccess && string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Failure result must contain an error message.");

        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string errorMessage) => new(false, errorMessage);
}
