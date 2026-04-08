namespace WorkTracker.Application.Authentication.Common;

public sealed record AuthResponse(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    string Token);
