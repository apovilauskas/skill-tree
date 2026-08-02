using FluentValidation;
using skill_tree.DTOs;

namespace skill_tree.Validators;

public class EditSkillDtoValidator : AbstractValidator<EditSkillDto>
{
    public EditSkillDtoValidator()
    {
        RuleFor(x => x.metric)
            .Matches("^[a-zA-Z0-9_ -]+$").WithMessage("Metric can only contain letters, numbers, hyphens, underscores and spaces")
            .When(x => !string.IsNullOrEmpty(x.metric));
    }
}