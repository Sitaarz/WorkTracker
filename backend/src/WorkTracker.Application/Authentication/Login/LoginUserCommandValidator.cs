using FluentValidation;

namespace WorkTracker.Application.Authentication.Login;

public class LoginUserCommandValidator: AbstractValidator<LogInUserCommand>
{
    public LoginUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
}
