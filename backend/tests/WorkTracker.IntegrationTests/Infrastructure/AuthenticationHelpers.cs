using System.Net.Http.Json;
using WorkTracker.Application.Authentication.Login;
using WorkTracker.Application.Authentication.Register;

namespace WorkTracker.IntegrationTests.Infrastructure;

public static class AuthenticationHelpers
{
    public const string DefaultEmail = "user@test.com";
    public const string DefaultPassword = "Password123!";
    public const string DefaultName = "Test User";

    public static async Task RegisterAsync(
        this HttpClient client,
        string email = DefaultEmail,
        string password = DefaultPassword,
        string name = DefaultName)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/register",
            new RegisterUserCommand(name, email, password));

        response.EnsureSuccessStatusCode();
    }

    public static async Task LoginAsync(
        this HttpClient client,
        string email = DefaultEmail,
        string password = DefaultPassword)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new LogInUserCommand(email, password));

        response.EnsureSuccessStatusCode();
    }
}
