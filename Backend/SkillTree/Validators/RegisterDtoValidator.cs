using FluentValidation;
using skill_tree.DTOs;

namespace skill_tree.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Matches("^[a-zA-Z0-9_ -]+$").WithMessage("Username can only contain letters, numbers, hyphens, underscores, and spaces");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}