namespace CoachBoard.Domain.Entities;

public class Routine
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
