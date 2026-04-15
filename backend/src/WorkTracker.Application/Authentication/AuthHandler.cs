using Microsoft.AspNetCore.Identity;
using WorkTracker.Application.Abstractions.Authentication;
using WorkTracker.Application.Authentication.Common;
using WorkTracker.Application.Authentication.Login;
using WorkTracker.Application.Authentication.Register;
using WorkTracker.Application.Common;
using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Application.Authentication;

public class AuthHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtGenerator _jwtGenerator;

    public AuthHandler(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IJwtGenerator jwtGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterUserCommand command)
    {
        var cleanedEmail = command.Email.Trim().ToLower();
        var existingUser = await _userRepository.GetUserByEmailAsync(cleanedEmail);

        if (existingUser is not null)
        {
            return Result<AuthResponse>.Failure("User with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Email = cleanedEmail,
            Role = UserRoles.User,
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, command.Password);
        await _userRepository.CreateUserAsync(user);

        var token = _jwtGenerator.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(user.Id, user.Name, user.Email, user.Role, token));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LogInUserCommand command)
    {
        var cleanedEmail = command.Email.Trim().ToLower();
        var user = await _userRepository.GetUserByEmailAsync(cleanedEmail);

        if (user is null)
        {
            return Result<AuthResponse>.Failure("Invalid email");
        }

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return Result<AuthResponse>.Failure("Invalid password");
        }

        var token = _jwtGenerator.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(user.Id, user.Name, user.Email, user.Role, token));
    }
}
