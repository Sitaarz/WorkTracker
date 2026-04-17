using WorkTracker.Application.Authentication.Register;

namespace WorkTracker.UnitTests.Application.Auth;

[TestFixture]
public class RegisterUserCommandValidatorTests
{
    private RegisterUserCommandValidator _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new RegisterUserCommandValidator();

    private static RegisterUserCommand ValidCommand(
        string name = "Jan Kowalski",
        string email = "jan@example.com",
        string password = "Abcdef1") => new(name, email, password);

    [Test]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var result = _sut.Validate(ValidCommand());

        Assert.That(result.IsValid, Is.True, string.Join(", ", result.Errors));
    }

    [TestCase("", "Name is required.")]
    [TestCase(" ", "Name is required.")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string name, string expectedMessage)
    {
        var result = _sut.Validate(ValidCommand(name: name));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Does.Contain(expectedMessage));
    }

    [Test]
    public void Validate_ShouldFail_WhenNameIsTooLong()
    {
        var longName = new string('a', 101);

        var result = _sut.Validate(ValidCommand(name: longName));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage),
            Does.Contain("Name must not exceed 100 characters."));
    }

    [TestCase("", "Email is required.")]
    [TestCase("not-an-email", "Invalid email format.")]
    [TestCase("@no-local.pl", "Invalid email format.")]
    public void Validate_ShouldFail_WhenEmailIsInvalid(string email, string expectedMessage)
    {
        var result = _sut.Validate(ValidCommand(email: email));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Does.Contain(expectedMessage));
    }

    [TestCase("", "Password is required.")]
    [TestCase("Ab1", "Password must be at least 6 characters long.")]
    [TestCase("abcdef1", "Password must contain at least one uppercase letter.")]
    [TestCase("ABCDEF1", "Password must contain at least one lowwercase letter.")]
    [TestCase("Abcdefg", "Password must contain at least one digit.")]
    public void Validate_ShouldFail_WhenPasswordDoesNotMeetRequirements(string password, string expectedMessage)
    {
        var result = _sut.Validate(ValidCommand(password: password));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Does.Contain(expectedMessage));
    }
}
