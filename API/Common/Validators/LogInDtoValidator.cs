using API.Resources;
using FluentValidation;
using SharedLibrary.Dtos;

namespace API.Common.Validators;

public class LogInDtoValidator : AbstractValidator<UserLogInDto>
{
    public LogInDtoValidator()
    {
        RuleFor(p => p.Email).EmailAddress().WithMessage(Messages.EmailMessage);
        RuleFor(p => p.Password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(512).WithMessage("This is a password, not an essay!")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .WithMessage(Messages.PasswordMessage);
    }
}
