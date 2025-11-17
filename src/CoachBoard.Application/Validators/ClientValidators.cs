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
    }
}
