using System.Net;
using System.Net.Http.Json;
using WorkTracker.API.Contracts.Auth;
using WorkTracker.Application.Authentication.Login;
using WorkTracker.Application.Authentication.Register;

namespace WorkTracker.IntegrationTests.API;

[TestFixture]
public class AuthTests : IntegrationTestBase
{
    [Test]
    public async Task Register_ShouldReturnOkAndSetAccessTokenCookie_WhenPayloadIsValid()
    {
        var command = new RegisterUserCommand("Anna", "anna@test.com", "Password123!");

        var response = await Client.PostAsJsonAsync("/api/v1/Auth/register", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies), Is.True);
        Assert.That(cookies!.Any(c => c.StartsWith("access_token=")), Is.True);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.That(body, Is.Not.Null);
        Assert.That(body!.User.Email, Is.EqualTo("anna@test.com"));
        Assert.That(body.User.Name, Is.EqualTo("Anna"));
        Assert.That(body.User.Roles, Does.Contain("User"));
    }

    [Test]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var command = new RegisterUserCommand("Anna", "dup@test.com", "Password123!");
        (await Client.PostAsJsonAsync("/api/v1/Auth/register", command)).EnsureSuccessStatusCode();

        var second = await Client.PostAsJsonAsync("/api/v1/Auth/register", command);

        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenPayloadIsInvalid()
    {
        var command = new RegisterUserCommand("", "not-an-email", "123");

        var response = await Client.PostAsJsonAsync("/api/v1/Auth/register", command);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Login_ShouldReturnOkAndSetCookie_WhenCredentialsAreValid()
    {
        await Client.RegisterAsync("login@test.com", "Password123!", "Login User");

        var response = await Client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new LogInUserCommand("login@test.com", "Password123!"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies), Is.True);
        Assert.That(cookies!.Any(c => c.StartsWith("access_token=")), Is.True);
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        await Client.RegisterAsync("bad@test.com", "Password123!");

        var response = await Client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new LogInUserCommand("bad@test.com", "wrong-password"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new LogInUserCommand("ghost@test.com", "Password123!"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Me_ShouldReturnUnauthorized_WhenNoAccessTokenCookie()
    {
        var response = await Client.GetAsync("/api/v1/Auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Me_ShouldReturnCurrentUser_WhenAuthenticated()
    {
        await Client.RegisterAsync("me@test.com", "Password123!", "Me User");

        var response = await Client.GetAsync("/api/v1/Auth/me");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.That(body!.User.Email, Is.EqualTo("me@test.com"));
        Assert.That(body.User.Name, Is.EqualTo("Me User"));
    }

    [Test]
    public async Task Logout_ShouldClearAccessTokenCookie()
    {
        await Client.RegisterAsync("logout@test.com");

        var response = await Client.PostAsync("/api/v1/Auth/logout", content: null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(response.Headers.TryGetValues("Set-Cookie", out var cookies), Is.True);
        Assert.That(
            cookies!.Any(c => c.StartsWith("access_token=") && c.Contains("expires=", StringComparison.OrdinalIgnoreCase)),
            Is.True,
            "Logout should set an expired access_token cookie.");
    }
}
