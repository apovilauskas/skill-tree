using FluentValidation;
using skill_tree.DTOs;

namespace skill_tree.Validators;

public class CreateSkillLogDtoValidator : AbstractValidator<CreateSkillLogDto>
{
    public CreateSkillLogDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");
        RuleFor(x => x.Note)
            .MaximumLength(256).WithMessage("Note must not exceed 256 characters");
    }
}
