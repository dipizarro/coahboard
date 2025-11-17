namespace CoachBoard.Domain.Entities;

public class RoutineExercise
{
    public int RoutineId { get; set; }
    public Routine Routine { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int Sets { get; set; }
    public int Reps { get; set; }
    public int Order { get; set; } = 1;
    public string? Notes { get; set; }
}
