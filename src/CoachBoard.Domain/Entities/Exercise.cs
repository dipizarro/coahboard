namespace CoachBoard.Domain.Entities;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = "General"; // Fuerza, Cardio, Movilidad, etc.
    public int? DefaultSets { get; set; }
    public int? DefaultReps { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
}
