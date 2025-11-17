using CoachBoard.Application.DTOs;
using FluentValidation;

namespace CoachBoard.Application.Validators;

public class ExerciseCreateDtoValidator : AbstractValidator<ExerciseCreateDto>
{
    public ExerciseCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(60);
        RuleFor(x => x.DefaultSets).InclusiveBetween(1, 20).When(x => x.DefaultSets.HasValue);
        RuleFor(x => x.DefaultReps).InclusiveBetween(1, 200).When(x => x.DefaultReps.HasValue);
    }
}

public class ExerciseUpdateDtoValidator : AbstractValidator<ExerciseUpdateDto>
{
    public ExerciseUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(60);
        RuleFor(x => x.DefaultSets).InclusiveBetween(1, 20).When(x => x.DefaultSets.HasValue);
        RuleFor(x => x.DefaultReps).InclusiveBetween(1, 200).When(x => x.DefaultReps.HasValue);
    }
}
