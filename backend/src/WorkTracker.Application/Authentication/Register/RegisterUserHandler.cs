using Microsoft.AspNetCore.Identity;
using WorkTracker.Application.Abstractions.Authentication;
using WorkTracker.Application.Authentication.Common;
using WorkTracker.Application.Common;
using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Application.Authentication.Register;

public class RegisterUserHandler
{
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IUserRepository _userRepository;
    public RegisterUserHandler(IPasswordHasher<User> passwordHasher, IJwtGenerator jwtGenerator, IUserRepository userRepository)
    {
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
        _userRepository = userRepository;
    }

    public async Task<Result<AuthResponse>> HandleAsync(RegisterUserCommand command)
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
        };

        var passwordHash = _passwordHasher.HashPassword(user, command.Password);
        user.PasswordHash = passwordHash;

        await _userRepository.CreateUserAsync(user);

        var token = _jwtGenerator.GenerateToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            token));
    }
}
