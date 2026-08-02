using FluentValidation;
using skill_tree.DTOs;

namespace skill_tree.Validators;

public class CreateSkillDtoValidator : AbstractValidator<CreateSkillDto>
{
    public CreateSkillDtoValidator()
    {
        RuleFor(x => x.Name)
         .NotEmpty().WithMessage("Name is required")
            .Matches("^[a-zA-Z0-9_ -]+$").WithMessage("Name can only contain letters, numbers, hyphens, and underscores");  
        RuleFor(x => x.Metric).NotEmpty().WithMessage("Metric is required");
        RuleFor(x => x.Target).GreaterThan(0).WithMessage("Target is required");
    }
}
