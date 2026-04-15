using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using WorkTracker.Application.Abstractions.Authentication;
using WorkTracker.Application.Authentication;
using WorkTracker.Application.Authentication.Login;
using WorkTracker.Application.Authentication.Register;
using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.UnitTests.Application.Auth;

[TestFixture]
public class AuthTests
{
    private IUserRepository _userRepository = null!;
    private IPasswordHasher<User> _passwordHasher = null!;
    private IJwtGenerator _jwtGenerator = null!;
    private AuthHandler _authHandler = null!;
    [SetUp]
    public void SetUp()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher<User>>();
        _jwtGenerator = Substitute.For<IJwtGenerator>();
        _authHandler = new AuthHandler(
            _userRepository,
            _passwordHasher,
            _jwtGenerator
        );
    }
    [Test]
    public async Task RegisterAsync_ShouldRegisterUser_WhenUserDoesNotExist()
    {
        // Arrange
        const string email = "test.name@example.com";
        const string jwtToken = "jwt-token";
        const string passwordHash = "hashed-password";
        var command = new RegisterUserCommand(
            Name: "TestName",
            Email: email,
            Password: "TestPassword123!"
        );
        _userRepository.GetUserByEmailAsync(email).Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<User>(), command.Password).Returns(passwordHash);
        _jwtGenerator.GenerateToken(Arg.Any<User>()).Returns(jwtToken);

        // Act
        var result = await _authHandler.RegisterAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Value, Is.Not.Null);

        Assert.That(result.Value.UserId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Value.Name, Is.EqualTo(command.Name));
        Assert.That(result.Value.Email, Is.EqualTo(email));
        Assert.That(result.Value.Role, Is.EqualTo(UserRoles.User));
        Assert.That(result.Value.Token, Is.EqualTo(jwtToken));

        await _userRepository.Received(1).CreateUserAsync(Arg.Is<User>(u =>
            u.Name == command.Name &&
            u.Email == command.Email &&
            u.Role == UserRoles.User &&
            u.PasswordHash == passwordHash));
    }
    [Test]
    public async Task RegisterAsync_ShouldNormalizeEmailAndUserName()
    {
        // Arrange
        const string email = "  Test.name@example.com";
        const string normalizedEmail = "test.name@example.com";
        const string name = "  TestName  ";
        const string normalizedName = "TestName";
        const string jwtToken = "jwt-token";
        var command = new RegisterUserCommand(
            Name: name,
            Email: email,
            Password: "TestPassword123!"
        );
        _userRepository.GetUserByEmailAsync(normalizedEmail).Returns((User?)null);
        _passwordHasher.HashPassword(Arg.Any<User>(), command.Password).Returns("hashed-password");
        _jwtGenerator.GenerateToken(Arg.Any<User>()).Returns(jwtToken);

        // Act
        var result = await _authHandler.RegisterAsync(command);
        
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.Email, Is.EqualTo(normalizedEmail));
        Assert.That(result.Value.Name, Is.EqualTo(normalizedName));

        await _userRepository.Received(1).GetUserByEmailAsync(normalizedEmail);
    }
    [Test]
    public async Task RegisterAsync_ShouldReturnError_WhenUserExists()
    {
        // Arrange
        const string email = "test.name@example.com";
        const string jwtToken = "jwt-token";
        const string passwordHash = "hashed-password";
        var command = new RegisterUserCommand(
            Name: "TestName",
            Email: email,
            Password: "TestPassword123!"
        );
        _userRepository.GetUserByEmailAsync(email).Returns(new User()
        {
            Name = command.Name,
            Email = email,
            Role = UserRoles.User,
            PasswordHash = passwordHash
        });
        _passwordHasher.HashPassword(Arg.Any<User>(), command.Password).Returns(passwordHash);
        _jwtGenerator.GenerateToken(Arg.Any<User>()).Returns(jwtToken);

        // Act
        var result = await _authHandler.RegisterAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.Value, Is.Null);

        await _userRepository.DidNotReceive().CreateUserAsync(Arg.Any<User>());
    }

    [Test]
    public async Task LoginAsync_ShouldLogInUser_WhenUserExists()
    {
        // Arrange
        const string email = "test.name@example.com";
        const string password = "test_password";
        const string jwtToken = "jwt-token";
        const string userName = "testUserName";
        const string passwordHash = "test-hash";
        var userId = Guid.NewGuid();
        LogInUserCommand command = new(email, password);

        _userRepository.GetUserByEmailAsync(email).Returns(new User()
        {
            Id = userId,
            Name = userName,
            Email = email,
            Role = UserRoles.User,
            PasswordHash = passwordHash
        });

        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), passwordHash, password)
            .Returns(PasswordVerificationResult.Success);
        _jwtGenerator.GenerateToken(Arg.Any<User>()).Returns(jwtToken);

        // Act
        var result = await _authHandler.LoginAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Value, Is.Not.Null);

        Assert.That(result.Value.UserId, Is.EqualTo(userId));
        Assert.That(result.Value.Email, Is.EqualTo(email));
        Assert.That(result.Value.Role, Is.EqualTo(UserRoles.User));
        Assert.That(result.Value.Name, Is.EqualTo(userName));
    }
    [Test]
    public async Task LoginAsync_ShouldReturnError_WhenPasswordIsIncorrect()
    {
        // Arrange
        const string email = "test.name@example.com";
        const string password = "test_password";
        const string userName = "testUserName";
        const string passwordHash = "test-hash";
        var userId = Guid.NewGuid();
        LogInUserCommand command = new(email, password);
        _userRepository.GetUserByEmailAsync(email).Returns(new User()
        {
            Id = userId,
            Name = userName,
            Email = email,
            Role = UserRoles.User,
            PasswordHash = passwordHash
        });
        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), passwordHash, password).Returns(PasswordVerificationResult.Failed);

        // Act
        var result = await _authHandler.LoginAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.Value, Is.Null);

        await _userRepository.Received(1).GetUserByEmailAsync(email);
    }
    [Test]
    public async Task LoginAsync_ShouldReturnError_WhenUserDoesNotExist()
    {
        // Arrange
        const string email = "test.name@example.com";
        const string password = "test_password";
        LogInUserCommand command = new(email, password);
        _userRepository.GetUserByEmailAsync(email).Returns((User?)null);

        // Act
        var result = await _authHandler.LoginAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.Value, Is.Null);

        await _userRepository.Received(1).GetUserByEmailAsync(email);
    }
}