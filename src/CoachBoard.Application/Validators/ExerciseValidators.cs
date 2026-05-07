using CoachBoard.Application.DTOs;
using FluentValidation;

namespace CoachBoard.Application.Validators;

public class ExerciseCreateDtoValidator : AbstractValidator<ExerciseCreateDto>
{
    public ExerciseCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(60);
        RuleFor(x => x.CoachId).GreaterThan(0).When(x => x.CoachId.HasValue);
        RuleFor(x => x.DefaultSets).InclusiveBetween(1, 20).When(x => x.DefaultSets.HasValue);
        RuleFor(x => x.DefaultReps).InclusiveBetween(1, 200).When(x => x.DefaultReps.HasValue);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Instructions).MaximumLength(2000);
        RuleFor(x => x.VideoUrl).MaximumLength(500);
        RuleFor(x => x.ReferenceUrl).MaximumLength(500);
        RuleFor(x => x.DifficultyLevel).MaximumLength(50);
        RuleFor(x => x.MovementPattern).MaximumLength(80);
        RuleFor(x => x.Equipment).MaximumLength(100);
        RuleFor(x => x.TargetMuscleGroup).MaximumLength(80);
        RuleFor(x => x.SecondaryMuscleGroups).MaximumLength(300);
        RuleFor(x => x.ExerciseType).MaximumLength(80);
        RuleFor(x => x.Environment).MaximumLength(80);
        RuleFor(x => x.Tags).MaximumLength(500);
    }
}

public class ExerciseUpdateDtoValidator : AbstractValidator<ExerciseUpdateDto>
{
    public ExerciseUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(60);
        RuleFor(x => x.CoachId).GreaterThan(0).When(x => x.CoachId.HasValue);
        RuleFor(x => x.DefaultSets).InclusiveBetween(1, 20).When(x => x.DefaultSets.HasValue);
        RuleFor(x => x.DefaultReps).InclusiveBetween(1, 200).When(x => x.DefaultReps.HasValue);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Instructions).MaximumLength(2000);
        RuleFor(x => x.VideoUrl).MaximumLength(500);
        RuleFor(x => x.ReferenceUrl).MaximumLength(500);
        RuleFor(x => x.DifficultyLevel).MaximumLength(50);
        RuleFor(x => x.MovementPattern).MaximumLength(80);
        RuleFor(x => x.Equipment).MaximumLength(100);
        RuleFor(x => x.TargetMuscleGroup).MaximumLength(80);
        RuleFor(x => x.SecondaryMuscleGroups).MaximumLength(300);
        RuleFor(x => x.ExerciseType).MaximumLength(80);
        RuleFor(x => x.Environment).MaximumLength(80);
        RuleFor(x => x.Tags).MaximumLength(500);
    }
}
