using System;
using System.Collections.Generic;
using System.Linq;

namespace CoachBoard.Application.DTOs;

public record RoutineItemDto(int ExerciseId, int Sets, int Reps, int Order, string? Notes);

public record RoutineCreateDto(string Title, int ClientId, List<RoutineItemDto> Items);
public record RoutineUpdateDto(string Title, List<RoutineItemDto> Items);

// DTOs de lectura convertidos a records con propiedades init y constructor por defecto
// para que AutoMapper pueda instanciarlos sin necesidad de usar el mapeo por constructor posicional.
public record RoutineReadItemDto
{
    public int ExerciseId { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int Sets { get; init; }
    public int Reps { get; init; }
    public int Order { get; init; }
    public string? Notes { get; init; }

    public RoutineReadItemDto() { }
}

public record RoutineReadDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int ClientId { get; init; }
    public DateTime CreatedAt { get; init; }

    public IEnumerable<RoutineReadItemDto> Items { get; init; } = Enumerable.Empty<RoutineReadItemDto>();

    public RoutineReadDto() { }
}