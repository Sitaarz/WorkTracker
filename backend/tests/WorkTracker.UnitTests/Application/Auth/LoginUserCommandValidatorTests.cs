using WorkTracker.Application.Authentication.Login;

namespace WorkTracker.UnitTests.Application.Auth;

[TestFixture]
public class LoginUserCommandValidatorTests
{
    private LoginUserCommandValidator _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new LoginUserCommandValidator();

    [Test]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var result = _sut.Validate(new LogInUserCommand("jan@example.com", "anything"));

        Assert.That(result.IsValid, Is.True, string.Join(", ", result.Errors));
    }

    [TestCase("", "Email is required.")]
    [TestCase("not-an-email", "Invalid email format.")]
    public void Validate_ShouldFail_WhenEmailIsInvalid(string email, string expectedMessage)
    {
        var result = _sut.Validate(new LogInUserCommand(email, "password"));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Does.Contain(expectedMessage));
    }

    [Test]
    public void Validate_ShouldFail_WhenPasswordIsEmpty()
    {
        var result = _sut.Validate(new LogInUserCommand("jan@example.com", ""));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Does.Contain("Password is required."));
    }
}
