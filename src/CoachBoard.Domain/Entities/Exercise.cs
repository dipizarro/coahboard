namespace CoachBoard.Domain.Entities;

public class Exercise
{
    public int Id { get; set; }
    public int? CoachId { get; set; }
    public Coach? Coach { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = "General"; // Fuerza, Cardio, Movilidad, etc.
    public int? DefaultSets { get; set; }
    public int? DefaultReps { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? ReferenceUrl { get; set; }
    public string? DifficultyLevel { get; set; }
    public string? MovementPattern { get; set; }
    public string? Equipment { get; set; }
    public string? TargetMuscleGroup { get; set; }
    public string? SecondaryMuscleGroups { get; set; }
    public string? ExerciseType { get; set; }
    public string? Environment { get; set; }
    public string? Tags { get; set; }
    public bool IsGlobal { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
    public ICollection<ExerciseMedia> Media { get; set; } = new List<ExerciseMedia>();
}
