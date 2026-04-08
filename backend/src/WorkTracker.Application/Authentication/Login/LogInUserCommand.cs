namespace WorkTracker.Application.Authentication.Login;

public sealed record class LogInUserCommand(
    string Email,
    string Password
);
