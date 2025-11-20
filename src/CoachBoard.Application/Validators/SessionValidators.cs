using CoachBoard.Application.DTOs;
using FluentValidation;

namespace CoachBoard.Application.Validators;

public static class SessionValidationConstants
{
    public static readonly string[] AllowedStatuses = { "Planned", "Done", "Canceled", "Missed" };
    public static readonly string[] AllowedTypes = { "Training", "PersonalBlock", "Other" };
}

public class SessionCreateDtoValidator : AbstractValidator<SessionCreateDto>
{
    public SessionCreateDtoValidator()
    {
        RuleFor(x => x.StartAt)
            .LessThan(x => x.EndAt)
            .WithMessage("StartAt debe ser menor que EndAt.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => SessionValidationConstants.AllowedTypes.Contains(t))
            .WithMessage($"Type debe ser uno de: {string.Join(", ", SessionValidationConstants.AllowedTypes)}.");

        RuleFor(x => x.Location)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Location));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class SessionUpdateDtoValidator : AbstractValidator<SessionUpdateDto>
{
    public SessionUpdateDtoValidator()
    {
        RuleFor(x => x.StartAt)
            .LessThan(x => x.EndAt)
            .WithMessage("StartAt debe ser menor que EndAt.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => SessionValidationConstants.AllowedTypes.Contains(t))
            .WithMessage($"Type debe ser uno de: {string.Join(", ", SessionValidationConstants.AllowedTypes)}.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => SessionValidationConstants.AllowedStatuses.Contains(s))
            .WithMessage($"Status debe ser uno de: {string.Join(", ", SessionValidationConstants.AllowedStatuses)}.");

        RuleFor(x => x.Location)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Location));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}

public class SessionStatusUpdateDtoValidator : AbstractValidator<SessionStatusUpdateDto>
{
    public SessionStatusUpdateDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => SessionValidationConstants.AllowedStatuses.Contains(s))
            .WithMessage($"Status debe ser uno de: {string.Join(", ", SessionValidationConstants.AllowedStatuses)}.");
    }
}
