using FluentValidation;
using skill_tree.DTOs;

namespace skill_tree.Validators;

public class PrerequisiteIdDtoValidator : AbstractValidator<PrerequisiteIdDto>
{
    public  PrerequisiteIdDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id is required");
    }
}