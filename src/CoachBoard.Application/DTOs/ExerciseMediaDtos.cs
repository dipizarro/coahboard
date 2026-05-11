namespace CoachBoard.Application.DTOs;

public record ExerciseMediaCreateDto(
    string MediaType,
    string Url,
    string? Title = null,
    string? Description = null);

public record ExerciseMediaReadDto(
    int Id,
    int ExerciseId,
    string MediaType,
    string Url,
    string? Title,
    string? Description,
    DateTime CreatedAt);
