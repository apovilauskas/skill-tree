using FluentValidation;
using skill_tree.DTOs;

namespace skill_tree.Validators;

public class EditSkillDtoValidator : AbstractValidator<EditSkillDto>
{
    public EditSkillDtoValidator()
    {
        RuleFor(x => x.metric)
            .MinimumLength(6).WithMessage("Length must be at least 6 characters")
            .MaximumLength(32).WithMessage("Length must not exceed 32 characters")
            .When(x => !string.IsNullOrEmpty(x.metric));
        RuleFor(x => x.description)
            .MaximumLength(256).WithMessage("Description must not exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.description));
        RuleFor(x => x.name)
            .MaximumLength(32).WithMessage("Name must not exceed 32 characters")
            .When(x => !string.IsNullOrEmpty(x.name));
    }
}