using FluentValidation;
using skill_tree.DTOs;

namespace skill_tree.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(6).WithMessage("Username must be at least 6 characters")
            .MaximumLength(32).WithMessage("Username must not exceed 32 characters");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
