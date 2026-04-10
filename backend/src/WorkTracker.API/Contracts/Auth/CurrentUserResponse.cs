namespace WorkTracker.API.Contracts.Auth;

public sealed record class CurrentUserResponse(Guid UserId, string Name, string Email, List<string> Role);
