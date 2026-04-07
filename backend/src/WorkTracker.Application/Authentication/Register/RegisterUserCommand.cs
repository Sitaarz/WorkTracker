namespace WorkTracker.Application.Authentication.Register;

public sealed record class RegisterUserCommand(
    string Name,
    string Email,
    string Password);
