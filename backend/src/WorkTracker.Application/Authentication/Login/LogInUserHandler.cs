using Microsoft.AspNetCore.Identity;
using WorkTracker.Application.Abstractions.Authentication;
using WorkTracker.Application.Authentication.Common;
using WorkTracker.Application.Common;

namespace WorkTracker.Application.Authentication.Login;

public class LogInUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<Infrastructure.Entities.User> _passwordHasher;
    private readonly IJwtGenerator _jwtGenerator;
    public LogInUserHandler(IUserRepository userRepository, IPasswordHasher<Infrastructure.Entities.User> passwordHasher, IJwtGenerator jwtGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
    }
    public async Task<Result<AuthResponse>> HandleAsync(LogInUserCommand command)
    {
        var cleanedEmail = command.Email.Trim().ToLower();

        var user = await _userRepository.GetUserByEmailAsync(cleanedEmail);
        if (user is null)
            return Result<AuthResponse>.Failure("Invalid email");

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
            return Result<AuthResponse>.Failure("Invalid password");

        var token = _jwtGenerator.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            token));
    }
}
