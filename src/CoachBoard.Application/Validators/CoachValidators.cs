using CoachBoard.Application.DTOs;
using FluentValidation;

namespace CoachBoard.Application.Validators;

public class CoachCreateDtoValidator : AbstractValidator<CoachCreateDto>
{
    public CoachCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120);

        RuleFor(x => x.Specialty)
            .NotEmpty().WithMessage("La especialidad es obligatoria.")
            .MaximumLength(80);
    }
}

public class CoachUpdateDtoValidator : AbstractValidator<CoachUpdateDto>
{
    public CoachUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(120);

        RuleFor(x => x.Specialty)
            .NotEmpty().MaximumLength(80);
    }
}
