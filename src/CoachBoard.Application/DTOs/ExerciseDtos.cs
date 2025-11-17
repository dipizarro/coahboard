namespace CoachBoard.Application.DTOs;

public record ExerciseCreateDto(string Name, string Category, int? DefaultSets, int? DefaultReps);
public record ExerciseUpdateDto(string Name, string Category, int? DefaultSets, int? DefaultReps);
public record ExerciseReadDto(int Id, string Name, string Category, int? DefaultSets, int? DefaultReps, DateTime CreatedAt);
