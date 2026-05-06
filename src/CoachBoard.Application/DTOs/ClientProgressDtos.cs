namespace CoachBoard.Application.DTOs;

public record ClientProgressCreateDto(
    DateTime RecordedAt,
    decimal? WeightKg = null,
    decimal? HeightCm = null,
    decimal? BodyFatPercentage = null,
    decimal? ChestCm = null,
    decimal? WaistCm = null,
    decimal? HipCm = null,
    decimal? LeftArmCm = null,
    decimal? RightArmCm = null,
    decimal? LeftThighCm = null,
    decimal? RightThighCm = null,
    int? RestingHeartRate = null,
    string? Notes = null);

public record ClientProgressUpdateDto(
    DateTime RecordedAt,
    decimal? WeightKg = null,
    decimal? HeightCm = null,
    decimal? BodyFatPercentage = null,
    decimal? ChestCm = null,
    decimal? WaistCm = null,
    decimal? HipCm = null,
    decimal? LeftArmCm = null,
    decimal? RightArmCm = null,
    decimal? LeftThighCm = null,
    decimal? RightThighCm = null,
    int? RestingHeartRate = null,
    string? Notes = null);

public record ClientProgressReadDto(
    int Id,
    int ClientId,
    DateTime RecordedAt,
    decimal? WeightKg,
    decimal? HeightCm,
    decimal? BodyFatPercentage,
    decimal? ChestCm,
    decimal? WaistCm,
    decimal? HipCm,
    decimal? LeftArmCm,
    decimal? RightArmCm,
    decimal? LeftThighCm,
    decimal? RightThighCm,
    int? RestingHeartRate,
    string? Notes,
    DateTime CreatedAt);
