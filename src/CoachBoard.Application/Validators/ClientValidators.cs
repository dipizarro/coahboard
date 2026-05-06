using CoachBoard.Application.DTOs;
using FluentValidation;

namespace CoachBoard.Application.Validators;

public class ClientCreateDtoValidator : AbstractValidator<ClientCreateDto>
{
    public ClientCreateDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(150);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Gender).MaximumLength(30);
        RuleFor(x => x.InitialHeightCm).GreaterThan(0).When(x => x.InitialHeightCm.HasValue);
        RuleFor(x => x.MainGoal).MaximumLength(150);
        RuleFor(x => x.ExperienceLevel).MaximumLength(50);
        RuleFor(x => x.MedicalNotes).MaximumLength(1000);
        RuleFor(x => x.InjuryNotes).MaximumLength(1000);
        RuleFor(x => x.GeneralNotes).MaximumLength(1000);
        RuleFor(x => x.CoachId).GreaterThan(0);
    }
}

public class ClientUpdateDtoValidator : AbstractValidator<ClientUpdateDto>
{
    public ClientUpdateDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).MaximumLength(150);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Gender).MaximumLength(30);
        RuleFor(x => x.InitialHeightCm).GreaterThan(0).When(x => x.InitialHeightCm.HasValue);
        RuleFor(x => x.MainGoal).MaximumLength(150);
        RuleFor(x => x.ExperienceLevel).MaximumLength(50);
        RuleFor(x => x.MedicalNotes).MaximumLength(1000);
        RuleFor(x => x.InjuryNotes).MaximumLength(1000);
        RuleFor(x => x.GeneralNotes).MaximumLength(1000);
    }
}
