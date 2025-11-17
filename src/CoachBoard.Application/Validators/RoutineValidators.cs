using CoachBoard.Application.DTOs;
using FluentValidation;

namespace CoachBoard.Application.Validators;

public class RoutineItemDtoValidator : AbstractValidator<RoutineItemDto>
{
    public RoutineItemDtoValidator()
    {
        RuleFor(x => x.ExerciseId).GreaterThan(0);
        RuleFor(x => x.Sets).InclusiveBetween(1, 20);
        RuleFor(x => x.Reps).InclusiveBetween(1, 500);
        RuleFor(x => x.Order).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class RoutineCreateDtoValidator : AbstractValidator<RoutineCreateDto>
{
    public RoutineCreateDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ClientId).GreaterThan(0);
        RuleFor(x => x.Items).NotNull().Must(items => items.Count > 0)
            .WithMessage("La rutina debe tener al menos 1 ejercicio.");

        RuleForEach(x => x.Items).SetValidator(new RoutineItemDtoValidator());
    }
}

public class RoutineUpdateDtoValidator : AbstractValidator<RoutineUpdateDto>
{
    public RoutineUpdateDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Items).NotNull().Must(items => items.Count > 0);
        RuleForEach(x => x.Items).SetValidator(new RoutineItemDtoValidator());
    }
}
