namespace CoachBoard.Application.DTOs;

public record RoutineItemDto(int ExerciseId, int Sets, int Reps, int Order, string? Notes);

public record RoutineCreateDto(string Title, int ClientId, List<RoutineItemDto> Items);
public record RoutineUpdateDto(string Title, List<RoutineItemDto> Items);

public record RoutineReadItemDto(int ExerciseId, string ExerciseName, string Category, int Sets, int Reps, int Order, string? Notes);

public record RoutineReadDto(int Id, string Title, int ClientId, DateTime CreatedAt, IEnumerable<RoutineReadItemDto> Items);
