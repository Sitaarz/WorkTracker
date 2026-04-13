namespace WorkTracker.API.Contracts.Auth;

public sealed record UserResponse(string Email, string Name, List<string> Roles);
