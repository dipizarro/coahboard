using CoachBoard.Application.DTOs;
using FluentValidation;

namespace CoachBoard.Application.Validators;

public class ClientProgressCreateDtoValidator : AbstractValidator<ClientProgressCreateDto>
{
    public ClientProgressCreateDtoValidator()
    {
        RuleFor(x => x.RecordedAt).NotEmpty();
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.HeightCm).GreaterThan(0).When(x => x.HeightCm.HasValue);
        RuleFor(x => x.BodyFatPercentage).InclusiveBetween(0, 100).When(x => x.BodyFatPercentage.HasValue);
        RuleFor(x => x.ChestCm).GreaterThan(0).When(x => x.ChestCm.HasValue);
        RuleFor(x => x.WaistCm).GreaterThan(0).When(x => x.WaistCm.HasValue);
        RuleFor(x => x.HipCm).GreaterThan(0).When(x => x.HipCm.HasValue);
        RuleFor(x => x.LeftArmCm).GreaterThan(0).When(x => x.LeftArmCm.HasValue);
        RuleFor(x => x.RightArmCm).GreaterThan(0).When(x => x.RightArmCm.HasValue);
        RuleFor(x => x.LeftThighCm).GreaterThan(0).When(x => x.LeftThighCm.HasValue);
        RuleFor(x => x.RightThighCm).GreaterThan(0).When(x => x.RightThighCm.HasValue);
        RuleFor(x => x.RestingHeartRate).InclusiveBetween(20, 250).When(x => x.RestingHeartRate.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class ClientProgressUpdateDtoValidator : AbstractValidator<ClientProgressUpdateDto>
{
    public ClientProgressUpdateDtoValidator()
    {
        RuleFor(x => x.RecordedAt).NotEmpty();
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.HeightCm).GreaterThan(0).When(x => x.HeightCm.HasValue);
        RuleFor(x => x.BodyFatPercentage).InclusiveBetween(0, 100).When(x => x.BodyFatPercentage.HasValue);
        RuleFor(x => x.ChestCm).GreaterThan(0).When(x => x.ChestCm.HasValue);
        RuleFor(x => x.WaistCm).GreaterThan(0).When(x => x.WaistCm.HasValue);
        RuleFor(x => x.HipCm).GreaterThan(0).When(x => x.HipCm.HasValue);
        RuleFor(x => x.LeftArmCm).GreaterThan(0).When(x => x.LeftArmCm.HasValue);
        RuleFor(x => x.RightArmCm).GreaterThan(0).When(x => x.RightArmCm.HasValue);
        RuleFor(x => x.LeftThighCm).GreaterThan(0).When(x => x.LeftThighCm.HasValue);
        RuleFor(x => x.RightThighCm).GreaterThan(0).When(x => x.RightThighCm.HasValue);
        RuleFor(x => x.RestingHeartRate).InclusiveBetween(20, 250).When(x => x.RestingHeartRate.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
