using FluentValidation;
using skill_tree.DTOs;

namespace skill_tree.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(32).WithMessage("Username must not exceed 32 characters")
            .MinimumLength(6).WithMessage("Username must be at least 6 characters");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}