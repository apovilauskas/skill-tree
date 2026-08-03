using FluentValidation;
using skill_tree.DTOs;

namespace skill_tree.Validators;

public class CreateSkillDtoValidator : AbstractValidator<CreateSkillDto>
{
    public CreateSkillDtoValidator()
    {
        RuleFor(x => x.Name)
         .NotEmpty().WithMessage("Name is required")
         .Length(6, 32).WithMessage("Name must be between 6 and 32 characters");
        RuleFor(x => x.Metric).NotEmpty().WithMessage("Metric is required")
            .MaximumLength(32);
        RuleFor(x => x.Target).GreaterThan(0).WithMessage("Target is required");
        RuleFor(x => x.Description)
            .MaximumLength(256).WithMessage("Description must not exceed 256 characters");
    }
}
